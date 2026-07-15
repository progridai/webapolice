import type { IdentidadeVisualOrganizacao, IdentidadeVisualCache } from './types';

// Função auxiliar para converter HEX em RGB
const hexToRgb = (hex: string) => {
  const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
  return result ? {
    r: parseInt(result[1], 16),
    g: parseInt(result[2], 16),
    b: parseInt(result[3], 16)
  } : null;
};

// Conversão RGB para HSL
const hexToHsl = (hex: string) => {
  const rgb = hexToRgb(hex);
  if (!rgb) return { h: 0, s: 0, l: 0 };
  const r = rgb.r / 255;
  const g = rgb.g / 255;
  const b = rgb.b / 255;

  const max = Math.max(r, g, b), min = Math.min(r, g, b);
  let h = 0, s = 0;
  const l = (max + min) / 2;

  if (max !== min) {
    const d = max - min;
    s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
    switch (max) {
      case r: h = (g - b) / d + (g < b ? 6 : 0); break;
      case g: h = (b - r) / d + 2; break;
      case b: h = (r - g) / d + 4; break;
    }
    h /= 6;
  }

  return { h, s, l };
};

// Conversão HSL para HEX
const hslToHex = (h: number, s: number, l: number) => {
  let r, g, b;

  if (s === 0) {
    r = g = b = l; // achromatic
  } else {
    const hue2rgb = (p: number, q: number, t: number) => {
      if (t < 0) t += 1;
      if (t > 1) t -= 1;
      if (t < 1/6) return p + (q - p) * 6 * t;
      if (t < 1/2) return q;
      if (t < 2/3) return p + (q - p) * (2/3 - t) * 6;
      return p;
    };

    const q = l < 0.5 ? l * (1 + s) : l + s - l * s;
    const p = 2 * l - q;
    r = hue2rgb(p, q, h + 1/3);
    g = hue2rgb(p, q, h);
    b = hue2rgb(p, q, h - 1/3);
  }

  const toHex = (x: number) => {
    const hex = Math.round(x * 255).toString(16);
    return hex.length === 1 ? '0' + hex : hex;
  };

  return `#${toHex(r)}${toHex(g)}${toHex(b)}`;
};

// Ajustar luminosidade mantendo o matiz
const ajustarLuminosidade = (hex: string, fator: number) => {
  const hsl = hexToHsl(hex);
  hsl.l = Math.max(0, Math.min(1, hsl.l + fator));
  return hslToHex(hsl.h, hsl.s, hsl.l);
};

// Calcular Luminância Relativa
const calcularLuminancia = (hex: string) => {
  const rgb = hexToRgb(hex);
  if (!rgb) return 0;
  
  const a = [rgb.r, rgb.g, rgb.b].map((v) => {
    v /= 255;
    return v <= 0.03928 ? v / 12.92 : Math.pow((v + 0.055) / 1.055, 2.4);
  });
  return a[0] * 0.2126 + a[1] * 0.7152 + a[2] * 0.0722;
};

// Selecionar cor de contraste adequada
export const obterCorContraste = (hexCorFundo: string) => {
  const luminancia = calcularLuminancia(hexCorFundo);
  return luminancia > 0.179 ? '#1A1A1A' : '#FFFFFF';
};

// Identidade Padrão WebApólice
export const IDENTIDADE_PADRAO: IdentidadeVisualOrganizacao = {
  organizacaoId: 'padrao',
  marcaPrincipal: '#D4AF37',
  marcaPrincipalHover: '#C59F2E',
  marcaPrincipalAtiva: '#B28E25',
  corSobreMarcaPrincipal: '#1A1A1A',
};

const STORAGE_KEY_PREFIX = 'webapolice-identidade-';
const CACHE_VERSAO = '1.0';
const CACHE_VALIDADE_MS = 7 * 24 * 60 * 60 * 1000; // 7 dias

export class IdentidadeVisualService {
  static getStorageKey(organizacaoId: string) {
    return `${STORAGE_KEY_PREFIX}${organizacaoId}`;
  }

  static validarIdentidadeVisual(configuracao: Partial<IdentidadeVisualOrganizacao>): IdentidadeVisualOrganizacao {
    const orgId = configuracao.organizacaoId || 'padrao';
    const marcaPrincipal = configuracao.marcaPrincipal || IDENTIDADE_PADRAO.marcaPrincipal;

    // Gerar cores derivadas (hover, ativa) se não forem fornecidas
    const hover = configuracao.marcaPrincipalHover || ajustarLuminosidade(marcaPrincipal, -0.05);
    const ativa = configuracao.marcaPrincipalAtiva || ajustarLuminosidade(marcaPrincipal, -0.10);

    // Garantir contraste acessível se não for fornecido
    const sobreMarca = configuracao.corSobreMarcaPrincipal || obterCorContraste(marcaPrincipal);

    return {
      ...configuracao,
      organizacaoId: orgId,
      marcaPrincipal,
      marcaPrincipalHover: hover,
      marcaPrincipalAtiva: ativa,
      corSobreMarcaPrincipal: sobreMarca,
    };
  }

  static aplicarIdentidadeVisual(configuracao: Partial<IdentidadeVisualOrganizacao>) {
    try {
      const identidadeValida = this.validarIdentidadeVisual(configuracao);
      const docEl = document.documentElement;
      
      docEl.style.setProperty('--cor-marca-principal', identidadeValida.marcaPrincipal);
      docEl.style.setProperty('--cor-marca-principal-hover', identidadeValida.marcaPrincipalHover!);
      docEl.style.setProperty('--cor-marca-principal-ativa', identidadeValida.marcaPrincipalAtiva!);
      docEl.style.setProperty('--cor-sobre-marca-principal', identidadeValida.corSobreMarcaPrincipal!);
      
      // Cor suave para fundos (aprox 12% de opacidade)
      docEl.style.setProperty('--cor-marca-principal-suave', `${identidadeValida.marcaPrincipal}20`);
      docEl.style.setProperty('--cor-marca-principal-borda', identidadeValida.marcaPrincipalHover!);
      docEl.style.setProperty('--cor-foco', identidadeValida.marcaPrincipal);

      // Persistir no cache local
      const cache: IdentidadeVisualCache = {
        organizacaoId: identidadeValida.organizacaoId,
        versao: CACHE_VERSAO,
        atualizadoEm: new Date().toISOString(),
        identidade: identidadeValida
      };

      localStorage.setItem(this.getStorageKey(identidadeValida.organizacaoId), JSON.stringify(cache));
      localStorage.setItem('webapolice-identidade-atual', identidadeValida.organizacaoId);

      return identidadeValida;
    } catch (e) {
      console.error('Falha ao aplicar identidade visual', e);
      return this.restaurarIdentidadePadrao();
    }
  }

  static restaurarIdentidadePadrao() {
    const docEl = document.documentElement;
    
    docEl.style.removeProperty('--cor-marca-principal');
    docEl.style.removeProperty('--cor-marca-principal-hover');
    docEl.style.removeProperty('--cor-marca-principal-ativa');
    docEl.style.removeProperty('--cor-sobre-marca-principal');
    docEl.style.removeProperty('--cor-marca-principal-suave');
    docEl.style.removeProperty('--cor-marca-principal-borda');
    docEl.style.removeProperty('--cor-foco');

    localStorage.removeItem('webapolice-identidade-atual');
    
    return IDENTIDADE_PADRAO;
  }

  static obterIdentidadeAtual(): IdentidadeVisualOrganizacao {
    try {
      const atualId = localStorage.getItem('webapolice-identidade-atual');
      if (atualId && atualId !== 'padrao') {
        const rawCache = localStorage.getItem(this.getStorageKey(atualId));
        if (rawCache) {
          const cache = JSON.parse(rawCache) as IdentidadeVisualCache;
          
          // Validações do cache
          if (cache.versao !== CACHE_VERSAO) {
            console.warn('Cache de identidade com versão incompatível. Ignorando.');
            this.restaurarIdentidadePadrao();
            return IDENTIDADE_PADRAO;
          }

          const dataAtualizacao = new Date(cache.atualizadoEm).getTime();
          if (Date.now() - dataAtualizacao > CACHE_VALIDADE_MS) {
            console.warn('Cache de identidade expirado. Ignorando.');
            this.restaurarIdentidadePadrao();
            return IDENTIDADE_PADRAO;
          }

          if (cache.identidade && cache.identidade.marcaPrincipal) {
            return cache.identidade;
          }
        }
      }
    } catch (e) {
      console.error('Erro ao ler identidade local', e);
    }
    return IDENTIDADE_PADRAO;
  }
}
