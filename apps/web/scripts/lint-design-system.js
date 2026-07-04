// scripts/lint-design-system.js
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const TARGET_DIRECTORIES = ['src/components', 'src/pages', 'src/features'];
const IGNORED_FILES = ['src/components/ui/Icons.tsx']; // Permite SVGs originais se necessário

// Expressões regulares refinadas para evitar falsos positivos (como seletores de ID como #root ou #seletor)
const CSS_HEX_REGEX = /:\s*(#[0-9A-Fa-f]{3,8}|rgba?\([^)]+\))/i;
const TSX_HEX_REGEX = /['"`](#[0-9A-Fa-f]{3,8}|rgba?\([^)]+\))['"`]/i;

let errorCount = 0;

function scanDirectory(dirPath) {
  if (!fs.existsSync(dirPath)) return;
  
  const entries = fs.readdirSync(dirPath, { withFileTypes: true });

  for (const entry of entries) {
    const fullPath = path.join(dirPath, entry.name);
    const relativePath = path.relative(path.join(__dirname, '..'), fullPath).replace(/\\/g, '/');

    if (entry.isDirectory()) {
      scanDirectory(fullPath);
    } else if (entry.isFile()) {
      const ext = path.extname(entry.name);
      if (['.css', '.tsx', '.ts'].includes(ext)) {
        if (IGNORED_FILES.includes(relativePath)) {
          continue;
        }
        checkFile(fullPath, relativePath, ext);
      }
    }
  }
}

function checkFile(filePath, relativePath, ext) {
  const content = fs.readFileSync(filePath, 'utf-8');
  const lines = content.split('\n');

  lines.forEach((line, index) => {
    // Ignora comentários de lint ou justificativas explícitas
    if (line.includes('exceção justificada') || line.includes('justificado') || line.includes('eslint-disable')) {
      return;
    }

    let match = null;
    if (ext === '.css') {
      match = line.match(CSS_HEX_REGEX);
    } else if (ext === '.tsx' || ext === '.ts') {
      match = line.match(TSX_HEX_REGEX);
    }

    if (match) {
      const colorValue = match[1];
      console.error(
        `\x1b[31m[ERRO DO DESIGN SYSTEM]\x1b[0m Cor fixa detectada no arquivo: \x1b[33m${relativePath}:${index + 1}\x1b[0m`
      );
      console.error(`  > Linha: ${line.trim()}`);
      console.error(`  > Valor proibido: ${colorValue}`);
      console.error(`  > Solução: Substitua pelo correspondente var(--cor-*) ou var(--palette-*).\n`);
      errorCount++;
    }
  });
}

console.log('Iniciando verificação de cores hexadecimais/rgb fixadas nos componentes...');
const rootPath = path.join(__dirname, '..');

TARGET_DIRECTORIES.forEach(dir => {
  scanDirectory(path.join(rootPath, dir));
});

if (errorCount > 0) {
  console.error(`\x1b[31mFalha: Encontradas ${errorCount} ocorrências de cores fixadas indevidamente.\x1b[0m`);
  process.exit(1);
} else {
  console.log('\x1b[32mSucesso: Nenhuma cor fixa em componentes/páginas encontrada. Conformidade de tokens de 100%!\x1b[0m');
  process.exit(0);
}
