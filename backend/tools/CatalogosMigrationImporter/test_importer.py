"""Offline CLI regression tests. Build the project first; no database is contacted."""
import hashlib
import json
import os
from pathlib import Path
import subprocess
import tempfile
import unittest


class ImporterTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.dll = Path(__file__).parent / 'bin/Debug/net10.0/CatalogosMigrationImporter.dll'
        files = {
            'clientes.jsonl': [
                {'Cliente': 1, 'Cli_Nome': 'Sintetico', 'Cli_CPF_11': '52998224725', 'Lotacao': 3714},
                {'Cliente': 2, 'Cli_Nome': 'Outro', 'Cli_CPF_11': None, 'Lotacao': 99},
            ],
            'cidades.jsonl': [{'Cidade': 1, 'Cid_Nome': 'Cidade sintetica', 'Cid_UF': 'RS'}],
            'bancos.jsonl': [{'Banco': 1, 'Ban_Nome': 'Banco sintetico', 'Ban_Codigo': '001'}],
            'lotacoes.jsonl': [
                {'Lotacao': 3714, 'Lot_Nome': None, 'Lot_Cidade': 999},
                {'Lotacao': 99, 'Lot_Nome': None},
            ],
            'orgaos-siape.jsonl': [{'Orgao': 1, 'Nome': 'Orgao sintetico', 'Codigo': '01'}],
        }
        for file, rows in files.items():
            (self.root / file).write_text('\n'.join(json.dumps(r) for r in rows), encoding='utf-8')
        self.manifest()

    def manifest(self):
        def digest(file):
            return hashlib.sha256((self.root / file).read_bytes()).hexdigest()
        manifest = {
            'targetDatabase': 'webapolice_teste', 'sourceSha256': digest('clientes.jsonl'),
            'catalogs': [{'file': f, 'sha256': digest(f)} for f in
                         ['cidades.jsonl', 'bancos.jsonl', 'lotacoes.jsonl', 'orgaos-siape.jsonl']],
        }
        (self.root / 'manifesto-carga.json').write_text(json.dumps(manifest), encoding='utf-8')

    def run_cli(self, apply=False):
        command = ['dotnet', str(self.dll.resolve()), '--source-dir', str(self.root),
                   '--output', str(self.root / 'resultado.json')]
        if apply:
            command.append('--apply')
        env = dict(os.environ, WEBAPOLICE_MIGRATION_TARGET='Host=localhost;Database=production_forbidden')
        return subprocess.run(command, capture_output=True, text=True, env=env)

    def test_plan_uses_confirmed_rules_and_only_referenced_allocations(self):
        result = self.run_cli()
        self.assertEqual(0, result.returncode, result.stderr)
        report = json.loads((self.root / 'resultado.json').read_text(encoding='utf-8-sig'))
        self.assertFalse(report['Applied'])
        self.assertEqual(1, report['Counts']['lotacoes.jsonl'])
        self.assertEqual('Lotação não informada', report['Allocation3714'])
        self.assertEqual([{'LegacyAllocationId': 3714, 'LegacyCityId': 999}], report['UnknownAllocationCities'])

    def test_changed_source_is_rejected(self):
        with (self.root / 'cidades.jsonl').open('a', encoding='utf-8') as stream:
            stream.write('\n')
        self.assertEqual(1, self.run_cli().returncode)
        self.assertFalse((self.root / 'resultado.json').exists())

    def test_existing_report_is_never_overwritten(self):
        (self.root / 'resultado.json').write_text('preservar', encoding='utf-8')
        self.assertEqual(1, self.run_cli().returncode)
        self.assertEqual('preservar', (self.root / 'resultado.json').read_text())

    def test_wrong_database_rejected_before_connecting(self):
        result = self.run_cli(apply=True)
        self.assertEqual(1, result.returncode)
        self.assertIn('somente em webapolice_teste', result.stderr)

    def test_duplicate_catalog_id_is_rejected(self):
        path = self.root / 'bancos.jsonl'
        path.write_text(path.read_text() + '\n' + path.read_text(), encoding='utf-8')
        self.manifest()
        self.assertEqual(1, self.run_cli().returncode)


if __name__ == '__main__':
    unittest.main()
