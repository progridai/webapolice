"""Gate completo local. Exige Docker e bases efêmeras; qualquer falha mantém exit code 1."""
from pathlib import Path
import json
import os
import subprocess
import sys
import time
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
LOGS = ROOT / "scratch" / "qualidade"
LOGS.mkdir(parents=True, exist_ok=True)
npm = "npm.cmd" if os.name == "nt" else "npm"
steps = [("segredos", [sys.executable, "backend/scripts/verificar-segredos.py"], ROOT)]
steps += [("frontend-" + name.replace(":", "-"), [npm, "run", name], ROOT / "apps/web") for name in
          ("format:check", "typecheck", "lint", "lint:design-system", "test:run", "build")]
steps += [
    ("npm-audit", [npm, "audit", "--audit-level=low"], ROOT / "apps/web"),
    ("restore", ["dotnet", "restore", "backend/WebApolice.slnx", "--configfile", "backend/NuGet.Config"], ROOT),
    ("backend-build", ["dotnet", "build", "backend/WebApolice.slnx", "--no-restore", "-c", "Release"], ROOT),
    ("backend-tests", ["dotnet", "test", "backend/WebApolice.slnx", "--no-build", "--no-restore", "-c", "Release", "--logger", "trx"], ROOT),
    ("nuget-audit", ["dotnet", "list", "backend/WebApolice.slnx", "package", "--vulnerable", "--include-transitive", "--format", "json", "--no-restore"], ROOT),
]

# Nunca reutilizar uma conexão externa opcional dos testes de catálogos.
env = dict(os.environ)
env.pop("WEBAPOLICE_CATALOGOS_TEST_CONNECTION", None)
env["Logging__EventLog__LogLevel__Default"] = "None"  # Só no processo de testes, sem EventLog do Windows.
results = []
for name, command, cwd in steps:
    logfile = LOGS / (name + ".log")
    started = time.time()
    with logfile.open("wb") as output:
        process = subprocess.run(command, cwd=cwd, env=env, stdout=output, stderr=subprocess.STDOUT)
    code = process.returncode
    if name == "backend-tests" and code == 0:
        reports = [p for p in ROOT.glob("backend/tests/*/TestResults/*.trx") if p.stat().st_mtime >= started]
        if not reports or any(int(ET.parse(p).find(".//{*}Counters").get("notExecuted", "0")) for p in reports):
            code = 1  # Integração ignorada não aprova o gate completo.
    if name == "nuget-audit" and code == 0:
        try:
            audit = json.loads(logfile.read_text(encoding="utf-8-sig"))
            if not audit.get("projects") or audit.get("problems") or any(
                package.get("vulnerabilities")
                for project in audit.get("projects", []) for framework in project.get("frameworks", [])
                for package in framework.get("topLevelPackages", []) + framework.get("transitivePackages", [])
            ):
                code = 1
        except (ValueError, UnicodeError):
            code = 1
    results.append({"check": name, "exitCode": code, "log": str(logfile.relative_to(ROOT))})
    print(f"{name}: {code}", flush=True)
(LOGS / "resultados.json").write_text(json.dumps(results, indent=2) + "\n", encoding="utf-8")
sys.exit(any(result["exitCode"] for result in results))
