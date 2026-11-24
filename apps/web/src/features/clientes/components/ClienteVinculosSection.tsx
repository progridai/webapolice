import { PropostasConsulta } from '../../propostas/components/PropostasConsulta';
import { useMemo, useState } from 'react';
import {
  Alert,
  Badge,
  Button,
  DetailsSection,
  FormField,
  Input,
  Modal,
  Pagination,
  Select,
} from '../../../components/ui';
import { useConsulta } from '../../../shared/hooks/useConsulta';
import {
  listarClienteVinculos,
  alterarSituacaoClienteVinculo,
  listarEstipulantesVinculo,
  salvarClienteVinculo,
  type ClienteVinculo,
} from '../api/clienteVinculosApi';

export function ClienteVinculosSection({
  clienteId,
  podeAlterar,
  podeConsultarPropostas = false,
}: {
  clienteId: string;
  podeAlterar: boolean;
  podeConsultarPropostas?: boolean;
}) {
  const [vinculoPropostas, setVinculoPropostas] =
    useState<ClienteVinculo | null>(null);
  const [propostasOcupadas, setPropostasOcupadas] = useState(false);
  const [situacaoEmAlteracao, setSituacaoEmAlteracao] = useState<string | null>(
    null,
  );
  const [erroSituacao, setErroSituacao] = useState<string | null>(null);
  const [pagina, setPagina] = useState(1);
  const [edicao, setEdicao] = useState<{ vinculo?: ClienteVinculo } | null>(
    null,
  );
  const load = useMemo(
    () => (signal: AbortSignal) =>
      listarClienteVinculos(clienteId, pagina, signal),
    [clienteId, pagina],
  );
  const { data, error, isLoading, refetch } = useConsulta(
    load,
    'Erro ao carregar vínculos.',
  );
  const alterarSituacao = async (vinculo: ClienteVinculo) => {
    if (situacaoEmAlteracao) return;
    setErroSituacao(null);
    setSituacaoEmAlteracao(vinculo.publicId);
    try {
      await alterarSituacaoClienteVinculo(
        clienteId,
        vinculo.publicId,
        !vinculo.ativo,
      );
      await refetch();
    } catch (error) {
      setErroSituacao(
        error instanceof Error
          ? error.message
          : 'Erro ao alterar situação do vínculo.',
      );
    } finally {
      setSituacaoEmAlteracao(null);
    }
  };
  return (
    <DetailsSection
      title="Vínculos"
      action={
        podeAlterar && (
          <Button size="small" onClick={() => setEdicao({})}>
            Adicionar vínculo
          </Button>
        )
      }
    >
      {erroSituacao && (
        <Alert variant="error" title="Não foi possível alterar a situação">
          {erroSituacao}
        </Alert>
      )}
      {isLoading && <p role="status">Carregando vínculos...</p>}
      {error && (
        <Alert variant="error" title="Erro ao carregar vínculos">
          {error.message}
          <Button onClick={() => void refetch()}>Tentar novamente</Button>
        </Alert>
      )}
      {!isLoading && !error && data?.items.length === 0 && (
        <p>Nenhum vínculo cadastrado.</p>
      )}
      <div className="flex flex-col gap-3">
        {data?.items.map((v) => (
          <div key={v.publicId} className="border-b border-borda pb-3">
            <div className="flex items-center justify-between gap-2">
              <span className="font-medium">
                Matrícula: {v.matricula || 'Não informada'}
              </span>
              <Badge variant={v.ativo ? 'success' : 'neutral'}>
                {v.ativo ? 'Ativo' : 'Inativo'}
              </Badge>
            </div>
            <p className="text-sm text-texto-secundario">
              Estipulante: {v.estipulante || 'Não informado'}
            </p>
            {v.apolices.length > 0 ? (
              <div className="mt-2 text-sm">
                <p className="font-medium">Apólices:</p>
                <ul
                  aria-label={`Apólices da matrícula ${v.matricula}`}
                  className="flex flex-col gap-1"
                >
                  {v.apolices.map((apolice) => (
                    <li key={apolice.publicId} className="break-words">
                      {apolice.nome ||
                        (apolice.numeros.length === 0
                          ? `Apólice ${apolice.publicId}`
                          : 'Apólice sem nome')}
                      {apolice.numeros.length > 0 &&
                        ` — Nº ${apolice.numeros.join(', ')}`}
                      {!apolice.ativa && ' · Apólice inativa'}
                      {!apolice.participacaoAtiva && ' · Participação inativa'}
                    </li>
                  ))}
                </ul>
              </div>
            ) : (
              <p className="mt-2 text-sm text-texto-secundario">
                Nenhuma apólice associada. Selecione este vínculo em Vidas da
                apólice.
              </p>
            )}
            {v.grupo && <p className="text-sm">Grupo: {v.grupo}</p>}
            {v.subgrupo && <p className="text-sm">Subgrupo: {v.subgrupo}</p>}
            {v.lotacao && <p className="text-sm">Lotação: {v.lotacao}</p>}
            {podeConsultarPropostas && (
              <Button
                variant="text"
                size="small"
                disabled={!!situacaoEmAlteracao || !!edicao}
                onClick={() => setVinculoPropostas(v)}
              >
                Abrir propostas do vínculo {v.matricula || 'sem matrícula'}
              </Button>
            )}
            {podeAlterar && (
              <Button
                variant="text"
                size="small"
                disabled={!!situacaoEmAlteracao}
                onClick={() => void alterarSituacao(v)}
              >
                {v.ativo ? 'Inativar' : 'Reativar'} vínculo {v.matricula}
              </Button>
            )}
            {podeAlterar && (
              <Button
                variant="text"
                size="small"
                onClick={() => setEdicao({ vinculo: v })}
              >
                Editar vínculo {v.matricula}
              </Button>
            )}
          </div>
        ))}
      </div>
      {data && data.totalCount > data.pageSize && (
        <Pagination
          currentPage={pagina}
          totalPages={Math.ceil(data.totalCount / data.pageSize)}
          onPageChange={setPagina}
          disabled={isLoading}
        />
      )}
      {vinculoPropostas && podeConsultarPropostas && (
        <Modal
          aberto
          size="large"
          title={`Propostas do vínculo ${vinculoPropostas.matricula || 'sem matrícula'}`}
          onClose={() => {
            if (!propostasOcupadas) setVinculoPropostas(null);
          }}
        >
          <PropostasConsulta
            key={`${clienteId}:${vinculoPropostas.publicId}`}
            clientePublicId={clienteId}
            clienteVinculoPublicId={vinculoPropostas.publicId}
            onBusy={setPropostasOcupadas}
          />
        </Modal>
      )}
      {edicao && (
        <VinculoEditor
          key={edicao.vinculo?.publicId || 'novo'}
          clienteId={clienteId}
          vinculo={edicao.vinculo}
          onClose={() => setEdicao(null)}
          onSaved={() => {
            setEdicao(null);
            void refetch();
          }}
        />
      )}
    </DetailsSection>
  );
}

function VinculoEditor({
  clienteId,
  vinculo,
  onClose,
  onSaved,
}: {
  clienteId: string;
  vinculo?: ClienteVinculo;
  onClose: () => void;
  onSaved: () => void;
}) {
  const [matricula, setMatricula] = useState(vinculo?.matricula || '');
  const [estipulante, setEstipulante] = useState(
    vinculo?.estipulantePublicId || '',
  );
  const [ativo, setAtivo] = useState(vinculo?.ativo ?? true);
  const [pagina, setPagina] = useState(1);
  const [saving, setSaving] = useState(false);
  const [erro, setErro] = useState<string | null>(null);
  const load = useMemo(
    () => (signal: AbortSignal) =>
      listarEstipulantesVinculo(clienteId, '', pagina, signal),
    [clienteId, pagina],
  );
  const consulta = useConsulta(load, 'Erro ao consultar estipulantes.');
  const salvar = async (event: React.FormEvent) => {
    event.preventDefault();
    if (saving) return;
    setErro(null);
    if (!estipulante || !matricula.trim()) {
      setErro('Informe estipulante e matrícula.');
      return;
    }
    setSaving(true);
    try {
      await salvarClienteVinculo(clienteId, vinculo?.publicId, {
        matricula,
        estipulantePublicId: estipulante,
        ativo,
      });
      onSaved();
    } catch (error) {
      setErro(
        error instanceof Error ? error.message : 'Erro ao salvar vínculo.',
      );
    } finally {
      setSaving(false);
    }
  };
  return (
    <Modal
      aberto
      title={vinculo ? 'Editar vínculo' : 'Adicionar vínculo'}
      onClose={() => {
        if (!saving) onClose();
      }}
      size="medium"
    >
      <form
        onSubmit={(event) => void salvar(event)}
        className="flex flex-col gap-3"
      >
        <p className="text-sm text-texto-secundario">
          Matrícula e estipulante podem ser corrigidos antes do uso. Quando
          houver participação ou proposta, cadastre outro vínculo para uma nova
          relação.
        </p>
        {erro && (
          <Alert variant="error" title="Não foi possível salvar">
            {erro}
          </Alert>
        )}
        {consulta.isLoading && <p role="status">Carregando estipulantes...</p>}
        {consulta.error && (
          <Alert variant="error" title="Erro ao consultar estipulantes">
            {consulta.error.message}
            <Button type="button" onClick={() => void consulta.refetch()}>
              Tentar novamente
            </Button>
          </Alert>
        )}
        <FormField label="Estipulante" required>
          <Select
            value={estipulante}
            onChange={(event) => setEstipulante(event.target.value)}
            disabled={saving || consulta.isLoading}
            required
          >
            <option value="">Selecione...</option>
            {vinculo?.estipulantePublicId &&
              !consulta.data?.items.some(
                (e) => e.publicId === vinculo.estipulantePublicId,
              ) && (
                <option value={vinculo.estipulantePublicId}>
                  {vinculo.estipulante}
                </option>
              )}
            {consulta.data?.items.map((e) => (
              <option key={e.publicId} value={e.publicId}>
                {e.nome}
              </option>
            ))}
          </Select>
        </FormField>
        {!consulta.isLoading &&
          !consulta.error &&
          consulta.data?.items.length === 0 && (
            <p>Nenhum estipulante encontrado.</p>
          )}
        {consulta.data && consulta.data.totalCount > consulta.data.pageSize && (
          <Pagination
            currentPage={pagina}
            totalPages={Math.ceil(
              consulta.data.totalCount / consulta.data.pageSize,
            )}
            onPageChange={setPagina}
            disabled={saving || consulta.isLoading}
          />
        )}
        <FormField label="Matrícula" required>
          <Input
            value={matricula}
            onChange={(event) => setMatricula(event.target.value)}
            maxLength={50}
            disabled={saving}
            required
          />
        </FormField>
        <FormField label="Situação">
          <Select
            value={ativo ? 'ativo' : 'inativo'}
            onChange={(event) => setAtivo(event.target.value === 'ativo')}
            disabled={saving}
          >
            <option value="ativo">Ativo</option>
            <option value="inativo">Inativo</option>
          </Select>
        </FormField>
        <div className="flex justify-end gap-2">
          <Button
            type="button"
            variant="text"
            onClick={onClose}
            disabled={saving}
          >
            Cancelar
          </Button>
          <Button
            type="submit"
            loading={saving}
            disabled={consulta.isLoading || !!consulta.error}
          >
            Salvar vínculo
          </Button>
        </div>
      </form>
    </Modal>
  );
}
