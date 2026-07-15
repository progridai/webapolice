import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { IdentidadeVisualService, IDENTIDADE_PADRAO, obterCorContraste } from './IdentidadeVisualService';

describe('IdentidadeVisualService', () => {
  beforeEach(() => {
    // Resetar o localStorage mock
    localStorage.clear();
    // Limpar estilos no root doc
    document.documentElement.style.cssText = '';
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('deve calcular corretamente a cor de contraste baseada na luminância', () => {
    // Cores extremas e seus contrastes garantidos (AAA/AA simulado pela luminância)
    const matrizCores = [
      { cor: '#D4AF37', esperado: '#1A1A1A' }, // Dourado padrão claro
      { cor: '#0055FF', esperado: '#FFFFFF' }, // Azul escuro
      { cor: '#00FF00', esperado: '#1A1A1A' }, // Verde Neon
      { cor: '#FF0000', esperado: '#1A1A1A' }, // Vermelho Vivo (Luminância 0.2126 > 0.179)
      { cor: '#6B21A8', esperado: '#FFFFFF' }, // Roxo Escuro
      { cor: '#FFFFFF', esperado: '#1A1A1A' }, // Branco Puro
      { cor: '#000000', esperado: '#FFFFFF' }, // Preto Absoluto
      { cor: '#FFFF00', esperado: '#1A1A1A' }, // Amarelo
    ];

    matrizCores.forEach(({ cor, esperado }) => {
      expect(obterCorContraste(cor)).toBe(esperado);
    });
  });

  it('deve validar e preencher campos faltantes ao aplicar uma nova identidade', () => {
    const configuracao = {
      organizacaoId: 'org-test',
      marcaPrincipal: '#0055FF'
    };

    const validada = IdentidadeVisualService.validarIdentidadeVisual(configuracao);

    expect(validada.organizacaoId).toBe('org-test');
    expect(validada.marcaPrincipal).toBe('#0055FF');
    // Cores derivadas devem ter sido geradas
    expect(validada.marcaPrincipalHover).toBeDefined();
    expect(validada.marcaPrincipalAtiva).toBeDefined();
    
    // Contraste para #0055FF (azul médio-escuro) deve ser branco
    expect(validada.corSobreMarcaPrincipal).toBe('#FFFFFF');
  });

  it('deve aplicar as variáveis CSS no elemento root', () => {
    const configuracao = {
      organizacaoId: 'org-123',
      marcaPrincipal: '#0055FF'
    };

    IdentidadeVisualService.aplicarIdentidadeVisual(configuracao);

    const docStyle = document.documentElement.style;
    expect(docStyle.getPropertyValue('--cor-marca-principal')).toBe('#0055FF');
    expect(docStyle.getPropertyValue('--cor-foco')).toBe('#0055FF');
    expect(docStyle.getPropertyValue('--cor-sobre-marca-principal')).toBe('#FFFFFF');
    // Verifica se salvou no localStorage
    expect(localStorage.getItem('webapolice-identidade-atual')).toBe('org-123');
  });

  it('deve restaurar para a identidade padrão', () => {
    // Aplicar uma identidade diferente primeiro
    IdentidadeVisualService.aplicarIdentidadeVisual({ organizacaoId: 'org-teste', marcaPrincipal: '#123456' });
    
    // Restaurar
    const padrao = IdentidadeVisualService.restaurarIdentidadePadrao();
    
    expect(padrao).toEqual(IDENTIDADE_PADRAO);
    expect(localStorage.getItem('webapolice-identidade-atual')).toBeNull();
    
    // As propriedades CSS injetadas devem ter sido removidas (retornando ao fallback do temas.css)
    const docStyle = document.documentElement.style;
    expect(docStyle.getPropertyValue('--cor-marca-principal')).toBe('');
    expect(docStyle.getPropertyValue('--cor-sobre-marca-principal')).toBe('');
  });

  it('deve carregar a identidade atual a partir do localStorage', () => {
    const config = {
      organizacaoId: 'org-555',
      marcaPrincipal: '#999999',
      marcaPrincipalHover: '#888888',
      marcaPrincipalAtiva: '#777777',
      corSobreMarcaPrincipal: '#1A1A1A'
    };

    const cachePayload = {
      organizacaoId: 'org-555',
      versao: '1.0',
      atualizadoEm: new Date().toISOString(),
      identidade: config
    };

    // Forçar persistência direta para simular inicialização
    localStorage.setItem('webapolice-identidade-atual', 'org-555');
    localStorage.setItem('webapolice-identidade-org-555', JSON.stringify(cachePayload));

    const obtida = IdentidadeVisualService.obterIdentidadeAtual();
    expect(obtida).toEqual(config);
  });

  it('deve descartar cache com versão incorreta ou expirado', () => {
    const config = { organizacaoId: 'org-old', marcaPrincipal: '#999999' };
    
    // Cenário 1: Versão inválida
    const cacheInvalido = {
      organizacaoId: 'org-old',
      versao: '0.9',
      atualizadoEm: new Date().toISOString(),
      identidade: config
    };
    localStorage.setItem('webapolice-identidade-atual', 'org-old');
    localStorage.setItem('webapolice-identidade-org-old', JSON.stringify(cacheInvalido));

    let obtida = IdentidadeVisualService.obterIdentidadeAtual();
    expect(obtida).toEqual(IDENTIDADE_PADRAO);

    // Cenário 2: Expirado (mais de 7 dias)
    const dataAntiga = new Date();
    dataAntiga.setDate(dataAntiga.getDate() - 8);
    
    const cacheExpirado = {
      organizacaoId: 'org-old',
      versao: '1.0',
      atualizadoEm: dataAntiga.toISOString(),
      identidade: config
    };
    localStorage.setItem('webapolice-identidade-atual', 'org-old');
    localStorage.setItem('webapolice-identidade-org-old', JSON.stringify(cacheExpirado));

    obtida = IdentidadeVisualService.obterIdentidadeAtual();
    expect(obtida).toEqual(IDENTIDADE_PADRAO);
  });
});
