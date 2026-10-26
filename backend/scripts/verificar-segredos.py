"""Scanner local de credenciais: informa somente arquivo/linha, nunca valores."""
from pathlib import Path
import re
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
EXTENSIONS = {".cs", ".json", ".md", ".ps1", ".sh", ".yml", ".yaml", ".config", ".sql", ".txt", ".ts", ".tsx", ".js", ".py"}


def inspect(files):
    findings = []
    for filename in sorted(set(files)):
        path = (ROOT / filename).resolve()
        if not path.is_relative_to(ROOT):
            raise ValueError("O scanner aceita somente arquivos dentro do repositório.")
        if not path.is_file() or path.suffix.lower() not in EXTENSIONS:
            continue
        raw = path.read_bytes()
        encoding = "utf-16" if raw.startswith((b"\xff\xfe", b"\xfe\xff")) else "utf-8-sig"
        for number, line in enumerate(raw.decode(encoding, errors="replace").splitlines(), 1):
            for match in re.finditer(r'(?i)(?:password|pwd)\s*=\s*([^;"\s]+)', line):
                value = match[1]
                if re.match(r"(?i)^\$|^\[string\]\$|^<|^sua_senha|^SENHA(?:_LOCAL)?$", value):
                    continue
                if "tests" in path.relative_to(ROOT).parts and re.fullmatch(r"'FiXtUrA![0-9]+';?", value):
                    continue
                test_local = ("tests" in path.relative_to(ROOT).parts or path.name == "appsettings.Testing.json") and re.search(
                    r"(?i)Host=(localhost|127\.0\.0\.1);", line
                ) and value in {"test", "postgres"}
                if not test_local:
                    findings.append((filename, number, "credencial literal de conexão"))
            if re.search(r"-----BEGIN (RSA |EC |OPENSSH )?PRIVATE KEY-----|\bAKIA[A-Z0-9]{16}\b", line):
                findings.append((filename, number, "possível chave privada ou credencial de acesso"))
            match = re.search(r'''(?i)["'](?:client_secret|clientSecret|password)["']\s*:\s*["']([^"']+)["']''', line)
            if match and not re.match(r"^\$|^<|^\{\{", match[1]):
                # Valores artificiais só são permitidos em fixtures de testes.
                if not ("tests" in path.relative_to(ROOT).parts and match[1] in {"test", "postgres"}):
                    findings.append((filename, number, "possível segredo literal em JSON"))
    return findings


if __name__ == "__main__":
    files = sys.argv[1:] or subprocess.check_output(
        ["git", "ls-files", "--cached", "--others", "--exclude-standard", "-z"], cwd=ROOT
    ).decode("utf-8").rstrip("\0").split("\0")
    findings = inspect(files)
    for filename, number, category in findings:
        print(f"{filename}:{number}: {category}")
    if findings:
        print(f"Scanner: {len(findings)} ocorrência(s); valores omitidos.")
    else:
        print("Scanner: nenhuma ocorrência nas regras locais; não substitui análise do histórico ou rotação.")
    sys.exit(bool(findings))
