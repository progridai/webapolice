"""Offline importer guard tests; no connection to a database is permitted."""
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess
import tempfile
import unittest


class GuardTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        folder = Path(__file__).resolve().parent
        self.dll = folder / 'bin/Debug/net10.0/ClientesMigrationImporter.dll'
        models = (folder.parent / 'ClientesMigrationSimulator/Models.cs').read_text(encoding='utf-8-sig')
        fields = re.findall(r'"([^"]+)"', models.split('RequiredFields = [', 1)[1].split('];', 1)[0])
        row = dict.fromkeys(fields)
        row.update(Cliente=9, Cli_Nome='Sintetico', Cli_CPF_11='52998224725', Cli_Falecido=False)
        (self.root / 'clientes.jsonl').write_text(json.dumps(row) + '\n', encoding='utf-8')
        self.manifest = {
            'targetDatabase': 'webapolice_teste',
            'sourceSha256': hashlib.sha256((self.root / 'clientes.jsonl').read_bytes()).hexdigest(),
            'authorizedPersonOverrides': {'435698': 1},
        }
        self.write_manifest()

    def write_manifest(self):
        (self.root / 'manifesto-carga.json').write_text(json.dumps(self.manifest), encoding='utf-8')

    def run_cli(self, *options):
        env = dict(os.environ, WEBAPOLICE_MIGRATION_TARGET='Host=localhost;Database=FORBIDDEN')
        return subprocess.run(['dotnet', str(self.dll), '--source-dir', str(self.root),
                               '--output', str(self.root / 'out'), *options], env=env,
                              capture_output=True, text=True)

    def test_production_connection_rejected_before_any_network_call(self):
        result = self.run_cli('--apply')
        self.assertEqual(1, result.returncode)
        self.assertIn('Somente webapolice_teste', result.stderr)

    def test_source_change_rejected(self):
        with (self.root / 'clientes.jsonl').open('a') as stream:
            stream.write('\n')
        result = self.run_cli('--apply')
        self.assertEqual(1, result.returncode)
        self.assertIn('origem mudou', result.stderr)

    def test_authorization_cannot_target_a_different_person(self):
        self.manifest['authorizedPersonOverrides'] = {'435698': 2}
        self.write_manifest()
        result = self.run_cli('--apply')
        self.assertEqual(1, result.returncode)
        self.assertIn('prioridade autorizada', result.stderr)

    def test_modes_are_mutually_exclusive(self):
        result = self.run_cli('--apply', '--validate-transaction')
        self.assertEqual(1, result.returncode)

    def test_old_output_is_preserved(self):
        (self.root / 'out').mkdir()
        (self.root / 'out/marker').write_text('preservar')
        self.assertEqual(1, self.run_cli('--apply').returncode)
        self.assertEqual('preservar', (self.root / 'out/marker').read_text())


if __name__ == '__main__':
    unittest.main()
