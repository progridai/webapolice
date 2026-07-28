import { httpClient } from '../../../services/http/httpClient';

export interface ModuloDto {
  publicId: string;
  codigo: string;
  nome: string;
  descricao: string;
  icone: string;
  ordem: number;
  ativo: boolean;
  habilitado: boolean;
}

export const listarModulos = async (): Promise<ModuloDto[]> => {
  const { data } = await httpClient.get<ModuloDto[]>('/api/seguranca/modulos');
  return data;
};

export const alterarHabilitacaoModulo = async (publicId: string, habilitado: boolean): Promise<void> => {
  await httpClient.put(`/api/seguranca/modulos/${publicId}/habilitacao`, { habilitado });
};
