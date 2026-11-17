import React, { useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../auth/useAuth';
import type { AuthUser } from '../../auth/auth.types';
import { useAppNavigation } from '../../app/navigation/useAppNavigation';
import { ROUTES } from '../../app/routes/routePaths';
import {
  Alert,
  Button,
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardFooter,
  Checkbox,
  EmptyState,
  Modal,
  PageHeader,
  SettingsIcon,
  Skeleton,
} from '../../components/ui';
import { useAtalhosHome } from './useAtalhosHome';

export const HomePage: React.FC = () => {
  const { user } = useAuth();
  return user ? <HomeConteudo key={user.id} user={user} /> : null;
};

function HomeConteudo({ user }: { user: AuthUser }) {
  const navigate = useNavigate();
  const groups = useAppNavigation()
    .map((group) => ({
      ...group,
      items: group.items.filter((item) => item.path !== ROUTES.APP),
    }))
    .filter((group) => group.items.length > 0);
  const preferences = useAtalhosHome(user.id);
  const [draft, setDraft] = useState<string[] | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const defaultPaths =
    groups
      .find((group) => group.label === 'Operação')
      ?.items.map((item) => item.path) ?? [];
  const selected = preferences.data?.atalhosHome ?? defaultPaths;
  const shortcuts = groups.flatMap((group) =>
    group.items
      .filter((item) => selected.includes(item.path))
      .map((item) => ({ ...item, groupLabel: group.label })),
  );

  useEffect(() => {
    document.title = 'Início | WebApólice';
  }, []);

  const close = useCallback(() => {
    if (!preferences.saving) setDraft(null);
  }, [preferences.saving, setDraft]);

  function open() {
    preferences.clearSaveError();
    setMessage(null);
    setDraft([...selected]);
  }

  async function save() {
    if (!draft) return;
    const paths = groups
      .flatMap((group) => group.items)
      .filter((item) => draft.includes(item.path))
      .map((item) => item.path);
    if (await preferences.save(paths)) {
      setDraft(null);
      setMessage('Seus atalhos foram atualizados.');
    }
  }

  const firstName = user.name?.split(' ')[0] || user.username;
  const ready =
    !preferences.isLoading && !preferences.error && preferences.data;

  return (
    <div className="flex flex-col gap-4 max-w-[1440px] mx-auto">
      <PageHeader
        title={`Olá, ${firstName}!`}
        description="Acesse suas telas favoritas ou personalize os atalhos da sua página inicial."
        actions={
          <Button variant="secondary" onClick={open} disabled={!ready}>
            <SettingsIcon size={16} />
            Personalizar atalhos
          </Button>
        }
      />
      {message && (
        <p role="status" className="text-sm text-texto-secundario">
          {message}
        </p>
      )}
      <section
        aria-labelledby="home-atalhos-titulo"
        className="flex flex-col gap-3"
      >
        <h2
          id="home-atalhos-titulo"
          className="text-lg font-semibold text-texto-principal"
        >
          Meus Atalhos
        </h2>
        {preferences.isLoading ? (
          <div
            role="status"
            aria-label="Carregando seus atalhos"
            className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4"
          >
            <Skeleton height={120} />
            <Skeleton height={120} />
            <Skeleton height={120} />
          </div>
        ) : preferences.error ? (
          <Alert variant="error" title="Não foi possível carregar seus atalhos">
            <p>{preferences.error.message}</p>
            <Button
              variant="secondary"
              className="mt-3"
              onClick={() => void preferences.refetch()}
            >
              Tentar novamente
            </Button>
          </Alert>
        ) : shortcuts.length === 0 ? (
          <EmptyState
            title="Nenhum atalho na sua página inicial"
            description={
              groups.length
                ? 'Use Personalizar atalhos para escolher as telas que deseja acessar por aqui.'
                : 'Nenhuma tela está disponível para seu acesso no momento.'
            }
          />
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
            {shortcuts.map((item) => {
              const Icon = item.icon;
              return (
                <Card key={item.path} className="flex flex-col">
                  <CardHeader>
                    <div className="flex items-center gap-2">
                      <span
                        className="text-texto-secundario"
                        aria-hidden="true"
                      >
                        <Icon size={18} />
                      </span>
                      <CardTitle>{item.label}</CardTitle>
                    </div>
                    <CardDescription>{item.groupLabel}</CardDescription>
                  </CardHeader>
                  <CardFooter className="mt-auto">
                    <Button
                      variant="secondary"
                      onClick={() => navigate(item.path)}
                      aria-label={`Abrir ${item.label}`}
                    >
                      Abrir tela
                    </Button>
                  </CardFooter>
                </Card>
              );
            })}
          </div>
        )}
      </section>
      <Modal
        aberto={draft !== null}
        onClose={close}
        title="Personalizar atalhos"
        footer={
          <>
            <Button
              variant="secondary"
              onClick={close}
              disabled={preferences.saving}
            >
              Cancelar
            </Button>
            <Button
              aria-label="Salvar atalhos"
              onClick={() => void save()}
              loading={preferences.saving}
            >
              Salvar atalhos
            </Button>
          </>
        }
      >
        <div className="flex flex-col gap-4">
          <p className="text-sm text-texto-secundario">
            Escolha as telas que deseja na sua página inicial. Sua seleção será
            salva para sua conta.
          </p>
          {preferences.saveError && (
            <Alert variant="error" title="Não foi possível salvar os atalhos">
              {preferences.saveError}
            </Alert>
          )}
          {groups.length === 0 && (
            <p className="text-sm text-texto-secundario">
              Nenhuma tela disponível para selecionar.
            </p>
          )}
          {groups.map((group) => (
            <fieldset
              key={group.label}
              disabled={preferences.saving}
              className="border-0 p-0 m-0 min-w-0"
            >
              <legend className="text-sm font-semibold text-texto-principal mb-2">
                {group.label}
              </legend>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                {group.items.map((item) => (
                  <Checkbox
                    key={item.path}
                    label={item.label}
                    checked={draft?.includes(item.path) ?? false}
                    onChange={(event) => {
                      const checked = event.target.checked;
                      setDraft((current) =>
                        current === null
                          ? null
                          : checked
                            ? [...current, item.path]
                            : current.filter((path) => path !== item.path),
                      );
                    }}
                  />
                ))}
              </div>
            </fieldset>
          ))}
        </div>
      </Modal>
    </div>
  );
}
