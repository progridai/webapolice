import { httpClient } from '../../../services/http/httpClient';

export interface RecursoDto {
  publicId: string;
  codigo: string;
  nome: string;
  ativo: boolean;
  habilitado: boolean;
}

export interface ModuloDto {
  publicId: string;
  codigo: string;
  nome: string;
  descricao: string;
  icone: string;
  ordem: number;
  ativo: boolean;
  habilitado: boolean;
  recursos?: RecursoDto[];
}

export const listarModulos = async (): Promise<ModuloDto[]> => {
  const { data } = await httpClient.get<ModuloDto[]>('/api/seguranca/modulos');
  return data;
};

export const alterarHabilitacaoModulo = async (publicId: string, habilitado: boolean): Promise<void> => {
  await httpClient.put(`/api/seguranca/modulos/${publicId}/habilitacao`, { habilitado });
};

export const alterarHabilitacaoRecurso = async (publicId: string, habilitado: boolean): Promise<void> => {
  await httpClient.put(`/api/seguranca/recursos/${publicId}/habilitacao`, { habilitado });
};
