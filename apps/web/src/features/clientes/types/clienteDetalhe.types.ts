export interface ClienteStatusResponse {
  codigo: string;
  nome: string;
}

export interface ClienteContatoResponse {
  tipo: string;
  valor: string;
  principal: boolean;
  ativo: boolean;
}

export interface ClienteEnderecoResponse {
  tipo: string;
  cep: string;
  logradouro: string;
  numero: string;
  complemento: string;
  bairro: string;
  cidade: string;
  cidadeId?: number;
  uf: string;
  principal: boolean;
  ativo: boolean;
}

export interface ClienteVinculoResponse {
  matricula: string;
  ativo: boolean;
  estipulante: string;
  subestipulante: string;
  grupo: string;
  subgrupo: string;
  lotacao: string;
}

export interface ClienteDependenteResponse {
  nome: string;
  tipoRelacao: string;
  documentoMascarado: string;
  dataNascimento?: string; // vindo como DateOnly no formato YYYY-MM-DD
}

export interface ClienteDetalheResponse {
  id: number;
  nome: string;
  documentoMascarado: string;
  status: ClienteStatusResponse;
  dataNascimento?: string;
  sexo?: number;
  falecido: boolean;
  dataObito?: string;
  contatos: ClienteContatoResponse[];
  enderecos: ClienteEnderecoResponse[];
  vinculos: ClienteVinculoResponse[];
  dependentes: ClienteDependenteResponse[];
}
