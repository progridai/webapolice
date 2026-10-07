import { useState } from 'react';
import { Modal, Button, FormField, Input, Alert } from '../../../components/ui';
import type { PremiosCobertura } from '../api/catalogos.api';

import { formatarPremio } from '../../../shared/utils/formatters';

function campoInicial(valor?: number | null) {
  return valor == null ? '' : valor.toFixed(2).replace('.', ',');
}

export function PremiosCoberturaModal({
  nome,
  valores,
  usarPadrao = false,
  padroes,
  onClose,
  onSalvar,
}: {
  nome: string;
  valores: PremiosCobertura;
  usarPadrao?: boolean;
  padroes?: PremiosCobertura;
  onClose: () => void;
  onSalvar: (valores: PremiosCobertura) => Promise<void>;
}) {
  const [titular, setTitular] = useState(campoInicial(valores.premioTitular));
  const [conjuge, setConjuge] = useState(campoInicial(valores.premioConjuge));
  const [salvando, setSalvando] = useState(false);
  const [error, setError] = useState('');
  const [errosCampos, setErrosCampos] = useState<Record<string, string>>({});
  async function salvar(e: React.FormEvent) {
    e.preventDefault();
    const erros: Record<string, string> = {};
    const ler = (valor: string, campo: string) => {
      if (!valor.trim() && usarPadrao) return null;
      if (!/^\d+(?:[.,]\d{1,2})?$/.test(valor.trim())) {
        erros[campo] =
          'Informe um valor não negativo, com até duas casas decimais.';
        return null;
      }
      const numero = Number(valor.trim().replace(',', '.'));
      if (!Number.isFinite(numero) || numero >= 1e16)
        erros[campo] = 'Valor acima do limite permitido.';
      return numero;
    };
    const dados = {
      premioTitular: ler(titular, 'titular'),
      premioConjuge: ler(conjuge, 'conjuge'),
    };
    setErrosCampos(erros);
    if (Object.keys(erros).length) return;
    setSalvando(true);
    setError('');
    try {
      await onSalvar(dados);
      onClose();
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Não foi possível salvar os prêmios.',
      );
    } finally {
      setSalvando(false);
    }
  }
  return (
    <Modal
      aberto
      title={usarPadrao ? 'Ajustar prêmios da Apólice' : 'Prêmios da Cobertura'}
      onClose={() => !salvando && onClose()}
      size="medium"
      footer={
        <>
          <Button variant="secondary" disabled={salvando} onClick={onClose}>
            Cancelar
          </Button>
          <Button
            type="submit"
            form="premios-cobertura-form"
            loading={salvando}
          >
            Salvar prêmios
          </Button>
        </>
      }
    >
      <form
        id="premios-cobertura-form"
        onSubmit={(e) => void salvar(e)}
        className="flex flex-col gap-3"
      >
        <p className="text-texto-principal">{nome}</p>
        {error && (
          <Alert variant="error" title="Não foi possível concluir">
            {error}
          </Alert>
        )}
        {usarPadrao && (
          <p className="text-texto-secundario">
            Deixe um campo em branco para usar o prêmio padrão do Plano.
          </p>
        )}
        <FormField
          label="Prêmio titular (R$)"
          required={!usarPadrao}
          error={errosCampos.titular}
          hint={
            usarPadrao
              ? `Padrão do Plano: ${formatarPremio(padroes?.premioTitular)}`
              : 'Use zero quando não houver cobrança.'
          }
        >
          <Input
            inputMode="decimal"
            value={titular}
            onChange={(e) => setTitular(e.target.value)}
            disabled={salvando}
          />
        </FormField>
        <FormField
          label="Prêmio cônjuge (R$)"
          required={!usarPadrao}
          error={errosCampos.conjuge}
          hint={
            usarPadrao
              ? `Padrão do Plano: ${formatarPremio(padroes?.premioConjuge)}`
              : 'Use zero quando não houver cobrança.'
          }
        >
          <Input
            inputMode="decimal"
            value={conjuge}
            onChange={(e) => setConjuge(e.target.value)}
            disabled={salvando}
          />
        </FormField>
      </form>
    </Modal>
  );
}
