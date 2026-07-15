export type IdentidadeVisualOrganizacao = {
  organizacaoId: string;
  nomeExibicao?: string;
  logotipoUrl?: string;
  faviconUrl?: string;
  marcaPrincipal: string;
  marcaPrincipalHover?: string;
  marcaPrincipalAtiva?: string;
  corSobreMarcaPrincipal?: string;
  temaInicial?: 'claro' | 'escuro' | 'sistema';
  versao?: string;
};

export type IdentidadeVisualCache = {
  organizacaoId: string;
  versao: string;
  atualizadoEm: string;
  identidade: IdentidadeVisualOrganizacao;
};
