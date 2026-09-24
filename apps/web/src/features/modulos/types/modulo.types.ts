export interface ModuloDto {
  publicId: string;
  nome: string;
  descricao?: string;
  ativo: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface ModuloListDto {
  publicId: string;
  nome: string;
  ativo: boolean;
  createdAt: string;
}

export interface CriarModuloDto {
  nome: string;
  descricao?: string;
}

export interface AtualizarModuloDto {
  nome: string;
  descricao?: string;
  ativo: boolean;
}
