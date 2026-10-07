import { ROUTES } from '../../app/routes/routePaths';
export interface CampoCatalogo {
  key: string;
  label: string;
  type?: 'number' | 'boolean' | 'textarea' | 'banco';
  maxLength?: number;
}
export interface ConfigCatalogo {
  recurso: string;
  permissao: string;
  titulo: string;
  singular: string;
  rota: string;
  secoes: { titulo: string; campos: CampoCatalogo[] }[];
}
export const CONVENIOS: ConfigCatalogo = {
  recurso: 'convenios-cobranca',
  permissao: 'convenios_cobranca',
  titulo: 'Convênios de Cobrança',
  singular: 'Convênio de Cobrança',
  rota: ROUTES.CONVENIOS_COBRANCA,
  secoes: [
    {
      titulo: 'Dados gerais e bancários',
      campos: [
        { key: 'nome', label: 'Nome', maxLength: 150 },
        { key: 'bancoCodigo', label: 'Banco', type: 'banco' },
        { key: 'agencia', label: 'Agência', maxLength: 30 },
        { key: 'contaCorrente', label: 'Conta corrente', maxLength: 30 },
        { key: 'nomeEmpresa', label: 'Nome da empresa', maxLength: 150 },
        { key: 'codigoEmpresa', label: 'Código da empresa', maxLength: 80 },
        {
          key: 'comunicaVindi',
          label: 'Comunicação com Vindi',
          type: 'boolean',
        },
        { key: 'observacao', label: 'Observação', type: 'textarea' },
      ],
    },
    {
      titulo: 'Arquivos de remessa e retorno',
      campos: [
        { key: 'numeroArquivo', label: 'Número do arquivo', type: 'number' },
        {
          key: 'nomeInicialArquivo',
          label: 'Nome inicial do arquivo',
          maxLength: 80,
        },
        { key: 'extensaoArquivo', label: 'Extensão do arquivo', maxLength: 10 },
        {
          key: 'layoutArquivo',
          label: 'Layout do arquivo (código)',
          type: 'number',
        },
        { key: 'localRemessaArquivo', label: 'Local da remessa' },
        { key: 'localRetornoArquivo', label: 'Local do retorno' },
      ],
    },
    {
      titulo: 'Dados e endereço do estabelecimento',
      campos: [
        { key: 'estNome', label: 'Nome', maxLength: 120 },
        {
          key: 'inscricaoEstadual',
          label: 'Inscrição estadual',
          maxLength: 40,
        },
        { key: 'estEndereco', label: 'Endereço', maxLength: 150 },
        { key: 'estNumero', label: 'Número', maxLength: 100 },
        { key: 'estBairro', label: 'Bairro', maxLength: 100 },
        { key: 'estComplemento', label: 'Complemento', maxLength: 100 },
        { key: 'estCep', label: 'CEP', maxLength: 20 },
        { key: 'estCidade', label: 'Cidade', maxLength: 100 },
        { key: 'estUf', label: 'UF', maxLength: 2 },
      ],
    },
  ],
};
export const COBERTURAS: ConfigCatalogo = {
  recurso: 'coberturas',
  permissao: 'coberturas',
  titulo: 'Coberturas',
  singular: 'Cobertura',
  rota: ROUTES.COBERTURAS,
  secoes: [
    {
      titulo: 'Dados da cobertura',
      campos: [
        { key: 'nome', label: 'Nome', maxLength: 150 },
        { key: 'nomeReduzido', label: 'Nome reduzido', maxLength: 30 },
        { key: 'basica', label: 'Classificação básica', maxLength: 50 },
        { key: 'reajuste', label: 'Reajuste', type: 'boolean' },
      ],
    },
  ],
};
export const PLANOS: ConfigCatalogo = {
  recurso: 'planos',
  permissao: 'planos',
  titulo: 'Planos',
  singular: 'Plano',
  rota: ROUTES.PLANOS,
  secoes: [
    {
      titulo: 'Dados do plano',
      campos: [
        { key: 'nome', label: 'Nome', maxLength: 150 },
        { key: 'ramo', label: 'Ramo', maxLength: 80 },
        { key: 'paga', label: 'Paga', type: 'boolean' },
        { key: 'reajuste', label: 'Reajuste', type: 'boolean' },
      ],
    },
  ],
};
