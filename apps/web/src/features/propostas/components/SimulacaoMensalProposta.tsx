import { useCallback, useState } from 'react';
import {
  Alert,
  Button,
  DescriptionItem,
  DescriptionList,
  DetailsSection,
  FormField,
  Select,
} from '../../../components/ui';
import { useConsulta } from '../../../shared/hooks/useConsulta';
import { formatarMoedaDecimal } from '../../../shared/utils/formatters';
import {
  obterRamosSimulacao,
  simularValorMensal,
} from '../api/simulacaoMensal.api';
import type { PropostaDetalhe } from '../types/proposta.types';

export function SimulacaoMensalProposta({
  proposta,
  disabled,
}: {
  proposta: PropostaDetalhe;
  disabled: boolean;
}) {
  const [aberta, setAberta] = useState(false);
  if (!proposta.podeSimularValorMensal) return null;
  return (
    <DetailsSection title="Simulação mensal">
      <p>
        Contratação de cônjuge:{' '}
        {proposta.incluiConjuge === null
          ? 'Não informada'
          : proposta.incluiConjuge
            ? 'Sim'
            : 'Não'}
        .
      </p>
      <p className="text-sm text-texto-secundario">
        Estimativa com os prêmios registrados na proposta e o IOF atual do Ramo
        escolhido. Não gera movimentos, títulos nem parcelas.
      </p>
      {proposta.incluiConjuge === null ? (
        <Alert variant="info" title="Informe a contratação de cônjuge">
          Preencha a escolha na pré proposta. Se já confirmada, prepare uma nova
          versão para conferência.
        </Alert>
      ) : (
        <Button
          variant="secondary"
          disabled={disabled}
          onClick={() => setAberta(!aberta)}
        >
          {aberta ? 'Fechar simulação' : 'Simular valor mensal'}
        </Button>
      )}
      {aberta && !disabled && (
        <ConsultaSimulacao
          key={`${proposta.publicId}:${proposta.revisaoPreenchimento}`}
          proposta={proposta}
        />
      )}
    </DetailsSection>
  );
}

function ConsultaSimulacao({ proposta }: { proposta: PropostaDetalhe }) {
  const [ramo, setRamo] = useState('');
  const [selecao, setSelecao] = useState<string | null>(null);
  const load = useCallback(
    (signal: AbortSignal) => obterRamosSimulacao(proposta.publicId, signal),
    [proposta.publicId],
  );
  const opcoes = useConsulta(load, 'Não foi possível carregar os Ramos.');
  const calcular = useCallback(
    (signal: AbortSignal) =>
      simularValorMensal(
        proposta.publicId,
        proposta.revisaoPreenchimento,
        selecao ?? '',
        signal,
      ),
    [proposta.publicId, proposta.revisaoPreenchimento, selecao],
  );
  const resultado = useConsulta(
    selecao ? calcular : null,
    'Não foi possível simular.',
  );
  return (
    <div className="flex flex-col gap-3">
      {opcoes.isLoading && <p role="status">Carregando Ramos…</p>}
      {opcoes.error && (
        <Alert variant="error" title="Falha ao carregar Ramos">
          {opcoes.error.message}
          <Button variant="secondary" onClick={() => void opcoes.refetch()}>
            Tentar novamente
          </Button>
        </Alert>
      )}
      {opcoes.data?.length === 0 && (
        <Alert variant="info" title="Nenhum Ramo disponível">
          Vincule um Ramo ativo à mesma Apólice para simular.
        </Alert>
      )}
      {!!opcoes.data?.length && (
        <form
          className="flex flex-col gap-3"
          onSubmit={(e) => {
            e.preventDefault();
            if (ramo) {
              if (selecao === ramo) void resultado.refetch();
              else setSelecao(ramo);
            }
          }}
        >
          <FormField label="Ramo para o IOF" required>
            <Select
              id="simulacao-ramo"
              placeholder="Selecione explicitamente um Ramo"
              required
              value={ramo}
              onChange={(e) => {
                setRamo(e.target.value);
                setSelecao(null);
              }}
            >
              {opcoes.data.map((r) => (
                <option key={r.publicId} value={r.publicId}>
                  {r.codigo} — {r.nome} — IOF{' '}
                  {r.iofPercentual === null
                    ? 'não informado'
                    : `${r.iofPercentual}%`}
                </option>
              ))}
            </Select>
          </FormField>
          <Button type="submit" disabled={!ramo || resultado.isLoading}>
            Calcular simulação
          </Button>
        </form>
      )}
      {resultado.isLoading && <p role="status">Calculando valor mensal…</p>}
      {resultado.error && (
        <Alert variant="error" title="Não foi possível simular">
          {resultado.error.message}
          <Button variant="secondary" onClick={() => void resultado.refetch()}>
            Tentar novamente
          </Button>
        </Alert>
      )}
      {resultado.data && !resultado.isLoading && !resultado.error && (
        <>
          <p>
            Revisão {resultado.data.revisaoPreenchimento}. Ramo:{' '}
            {resultado.data.ramo.nome}. IOF utilizado:{' '}
            {resultado.data.ramo.iofPercentual}%. Calculado em{' '}
            {new Date(resultado.data.calculadaEm).toLocaleString('pt-BR')}.
          </p>
          {resultado.data.coberturas.map((c) => (
            <DescriptionList key={c.publicId}>
              <DescriptionItem
                label="Cobertura"
                value={`${c.nome ?? 'Sem nome'}${c.premioAjustado ? ' (prêmio ajustado)' : ''}`}
              />
              <DescriptionItem
                label="Líquido titular"
                value={formatarMoedaDecimal(c.premioTitular)}
              />
              <DescriptionItem
                label="Líquido cônjuge considerado"
                value={formatarMoedaDecimal(c.premioConjuge)}
              />
              <DescriptionItem
                label="Líquido da cobertura"
                value={formatarMoedaDecimal(c.liquido)}
              />
            </DescriptionList>
          ))}
          <DescriptionList>
            <DescriptionItem
              label="Total líquido titular"
              value={formatarMoedaDecimal(resultado.data.liquidoTitular)}
            />
            <DescriptionItem
              label="Total líquido cônjuge"
              value={formatarMoedaDecimal(resultado.data.liquidoConjuge)}
            />
            <DescriptionItem
              label="Líquido total mensal"
              value={formatarMoedaDecimal(resultado.data.liquidoTotal)}
            />
            <DescriptionItem
              label="IOF mensal"
              value={formatarMoedaDecimal(resultado.data.iof)}
            />
            <DescriptionItem
              label="Total mensal simulado"
              value={formatarMoedaDecimal(resultado.data.totalMensal)}
            />
          </DescriptionList>
        </>
      )}
    </div>
  );
}
