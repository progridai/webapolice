import { useState } from 'react';
import {
  Alert,
  Button,
  FormField,
  Input,
  Pagination,
  ReadOnlyField,
  Select,
} from '../../../../components/ui';
import {
  useVinculosVida,
  type VinculoVidaOpcao,
} from '../../hooks/useVinculosVida';
export function VinculoVidaSelect({
  apoliceId,
  clienteId,
  value,
  onChange,
  atual,
  bloqueado,
  disabled,
  error,
  required,
  novaMatricula,
  onNovaMatriculaChange,
  podeCadastrar,
  estipulanteNome,
  matriculaError,
}: {
  apoliceId: string;
  clienteId: string;
  value: string;
  onChange: (id: string, requerEstipulante: boolean) => void;
  atual?: VinculoVidaOpcao;
  bloqueado: boolean;
  disabled: boolean;
  error?: string;
  required: boolean;
  novaMatricula: string | null;
  onNovaMatriculaChange: (matricula: string | null) => void;
  podeCadastrar: boolean;
  estipulanteNome?: string;
  matriculaError?: string;
}) {
  const [pagina, setPagina] = useState(1);
  const [selecionado, setSelecionado] = useState(atual);
  const {
    data,
    isLoading,
    error: consultaErro,
    refetch,
  } = useVinculosVida(
    apoliceId,
    bloqueado || novaMatricula !== null ? '' : clienteId,
    pagina,
  );
  if (bloqueado)
    return (
      <FormField
        label="Vínculo cadastral do cliente"
        hint="Esta participação possui propostas. O vínculo está preservado para manter o histórico."
      >
        <ReadOnlyField
          value={
            atual ? `Matrícula ${atual.matricula}` : 'Vínculo não informado'
          }
        />
      </FormField>
    );
  if (novaMatricula !== null)
    return (
      <div className="flex flex-col gap-2">
        <FormField label="Estipulante do vínculo">
          <ReadOnlyField
            value={estipulanteNome || 'Estipulante desta apólice'}
          />
        </FormField>
        <FormField
          label="Nova matrícula"
          required
          error={matriculaError}
          hint="O vínculo será cadastrado no estipulante desta apólice ao salvar a participação."
        >
          <Input
            value={novaMatricula}
            maxLength={50}
            disabled={disabled || !podeCadastrar}
            error={!!matriculaError}
            onChange={(event) => onNovaMatriculaChange(event.target.value)}
          />
        </FormField>
        <Button
          type="button"
          variant="text"
          disabled={disabled}
          onClick={() => onNovaMatriculaChange(null)}
        >
          Selecionar vínculo existente
        </Button>
      </div>
    );
  const opcoes = data?.items ?? [];
  const opcaoSelecionada =
    opcoes.find((v) => v.publicId === value) ?? selecionado;
  return (
    <div className="flex flex-col gap-2">
      <FormField
        label="Vínculo cadastral do cliente"
        required={required}
        error={error}
        hint="Selecione a matrícula do cliente no estipulante desta apólice."
      >
        <Select
          value={value}
          disabled={disabled || !clienteId || isLoading || !!consultaErro}
          onChange={(event) => {
            const id = event.target.value;
            const opcao = opcoes.find((v) => v.publicId === id);
            setSelecionado(opcao ?? atual);
            onChange(id, !!opcao?.requerEstipulante);
          }}
        >
          <option value="">
            {clienteId
              ? 'Selecione o vínculo...'
              : 'Selecione primeiro o cliente'}
          </option>
          {value && !opcoes.some((v) => v.publicId === value) && (
            <option value={value}>
              Matrícula{' '}
              {selecionado?.matricula || atual?.matricula || 'selecionada'} —
              vínculo atual
            </option>
          )}
          {opcoes.map((v) => (
            <option
              key={v.publicId}
              value={v.publicId}
              disabled={!!v.requerEstipulante && !podeCadastrar}
            >
              Matrícula {v.matricula} —{' '}
              {v.requerEstipulante
                ? 'Sem estipulante: vincular ao salvar'
                : v.estipulante}
            </option>
          ))}
        </Select>
      </FormField>
      {value && opcaoSelecionada?.requerEstipulante && (
        <Alert variant="info" title="Completar vínculo ao salvar">
          A matrícula {opcaoSelecionada.matricula} será vinculada a{' '}
          {estipulanteNome || 'este estipulante da apólice'} ao salvar a
          participação.
        </Alert>
      )}
      {isLoading && <p role="status">Carregando vínculos...</p>}
      {consultaErro && (
        <Alert variant="error" title="Erro ao carregar vínculos">
          {consultaErro.message}
          <Button type="button" onClick={() => void refetch()}>
            Tentar novamente
          </Button>
        </Alert>
      )}
      {clienteId && !isLoading && !consultaErro && data?.totalCount === 0 && (
        <p className="text-sm text-texto-secundario">
          Nenhum vínculo ativo compatível com o estipulante desta apólice.
        </p>
      )}
      {podeCadastrar && (
        <Button
          type="button"
          variant="text"
          disabled={disabled || !clienteId}
          onClick={() => onNovaMatriculaChange('')}
        >
          Cadastrar nova matrícula
        </Button>
      )}
      {!podeCadastrar && clienteId && (
        <p className="text-sm text-texto-secundario">
          Para cadastrar uma matrícula ou completar o estipulante, é necessária
          permissão para alterar clientes.
        </p>
      )}
      {data && data.totalCount > data.pageSize && (
        <Pagination
          currentPage={pagina}
          totalPages={Math.ceil(data.totalCount / data.pageSize)}
          onPageChange={setPagina}
          disabled={disabled || isLoading}
        />
      )}
    </div>
  );
}
