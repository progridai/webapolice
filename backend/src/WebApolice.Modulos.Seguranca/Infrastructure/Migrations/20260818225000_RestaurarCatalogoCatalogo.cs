using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

#nullable disable

namespace WebApolice.Modulos.Seguranca.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(SegurancaDbContext))]
    [Migration("20260818225000_RestaurarCatalogoCatalogo")]
    public partial class RestaurarCatalogoCatalogo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Migration corretiva: restaura o catálogo de segurança completo.
            // O banco de desenvolvimento (webapolice_teste) foi reconstruído do zero
            // e nunca recebeu os módulos CLIENTES, ESTIPULANTES e SEGURANCA,
            // nem os perfis base ADMINISTRADOR e ADMINISTRATIVO.
            // Esta migration é totalmente idempotente via UPSERT ON CONFLICT.

            migrationBuilder.Sql(@"
DO $$
DECLARE
    v_modulo_clientes_id    bigint;
    v_modulo_estipulantes_id bigint;
    v_modulo_seguranca_id   bigint;

    v_recurso_clientes_id       bigint;
    v_recurso_estipulantes_id   bigint;
    v_recurso_seg_usuarios_id   bigint;
    v_recurso_seg_perfis_id     bigint;
    v_recurso_seg_catalogo_id   bigint;
    v_recurso_seg_auditoria_id  bigint;

    v_perfil_admin_id     bigint;
    v_perfil_admtivo_id   bigint;
BEGIN

-- ============================================================
-- MÓDULO: CLIENTES
-- ============================================================
INSERT INTO seguranca.modulo (public_id, nome, codigo, ativo, habilitado, ordem)
VALUES ('81820600-0000-0000-0000-000000000001', 'Clientes', 'CLIENTES', true, true, 10)
ON CONFLICT (codigo) DO UPDATE SET nome = EXCLUDED.nome
RETURNING id INTO v_modulo_clientes_id;

INSERT INTO seguranca.recurso (public_id, modulo_id, nome, codigo, ativo, habilitado)
VALUES ('81820600-0000-0000-0000-000000000002', v_modulo_clientes_id, 'Clientes', 'CLIENTES', true, true)
ON CONFLICT (modulo_id, codigo) DO UPDATE SET nome = EXCLUDED.nome
RETURNING id INTO v_recurso_clientes_id;

INSERT INTO seguranca.permissao (public_id, recurso_id, nome, codigo, ativo)
VALUES
    ('81820600-0000-0000-0001-000000000001', v_recurso_clientes_id, 'Visualizar', 'clientes.visualizar', true),
    ('81820600-0000-0000-0001-000000000002', v_recurso_clientes_id, 'Inserir',    'clientes.inserir',    true),
    ('81820600-0000-0000-0001-000000000003', v_recurso_clientes_id, 'Alterar',    'clientes.alterar',    true),
    ('81820600-0000-0000-0001-000000000004', v_recurso_clientes_id, 'Inativar',   'clientes.inativar',   true),
    ('81820600-0000-0000-0001-000000000005', v_recurso_clientes_id, 'Reativar',   'clientes.reativar',   true)
ON CONFLICT (codigo) DO UPDATE SET nome = EXCLUDED.nome;

-- ============================================================
-- MÓDULO: ESTIPULANTES
-- ============================================================
INSERT INTO seguranca.modulo (public_id, nome, codigo, ativo, habilitado, ordem)
VALUES ('81820601-0000-0000-0000-000000000001', 'Estipulantes', 'ESTIPULANTES', true, true, 12)
ON CONFLICT (codigo) DO UPDATE SET nome = EXCLUDED.nome
RETURNING id INTO v_modulo_estipulantes_id;

INSERT INTO seguranca.recurso (public_id, modulo_id, nome, codigo, ativo, habilitado)
VALUES ('81820601-0000-0000-0000-000000000002', v_modulo_estipulantes_id, 'Estipulantes', 'ESTIPULANTES', true, true)
ON CONFLICT (modulo_id, codigo) DO UPDATE SET nome = EXCLUDED.nome
RETURNING id INTO v_recurso_estipulantes_id;

INSERT INTO seguranca.permissao (public_id, recurso_id, nome, codigo, ativo)
VALUES
    ('81820601-0000-0000-0001-000000000001', v_recurso_estipulantes_id, 'Visualizar', 'estipulantes.visualizar', true),
    ('81820601-0000-0000-0001-000000000002', v_recurso_estipulantes_id, 'Inserir',    'estipulantes.inserir',    true),
    ('81820601-0000-0000-0001-000000000003', v_recurso_estipulantes_id, 'Alterar',    'estipulantes.alterar',    true),
    ('81820601-0000-0000-0001-000000000004', v_recurso_estipulantes_id, 'Excluir',    'estipulantes.excluir',    true),
    ('81820601-0000-0000-0001-000000000005', v_recurso_estipulantes_id, 'Inativar',   'estipulantes.inativar',   true),
    ('81820601-0000-0000-0001-000000000006', v_recurso_estipulantes_id, 'Reativar',   'estipulantes.reativar',   true)
ON CONFLICT (codigo) DO UPDATE SET nome = EXCLUDED.nome;

-- ============================================================
-- MÓDULO: SEGURANCA
-- ============================================================
INSERT INTO seguranca.modulo (public_id, nome, codigo, ativo, habilitado, ordem)
VALUES ('81820602-0000-0000-0000-000000000001', 'Segurança', 'SEGURANCA', true, true, 90)
ON CONFLICT (codigo) DO UPDATE SET nome = EXCLUDED.nome
RETURNING id INTO v_modulo_seguranca_id;

-- Recurso: Usuários
INSERT INTO seguranca.recurso (public_id, modulo_id, nome, codigo, ativo, habilitado)
VALUES ('81820602-0000-0000-0001-000000000001', v_modulo_seguranca_id, 'Usuários', 'SEGURANCA_USUARIOS', true, true)
ON CONFLICT (modulo_id, codigo) DO UPDATE SET nome = EXCLUDED.nome
RETURNING id INTO v_recurso_seg_usuarios_id;

INSERT INTO seguranca.permissao (public_id, recurso_id, nome, codigo, ativo)
VALUES
    ('81820602-0001-0000-0001-000000000001', v_recurso_seg_usuarios_id, 'Visualizar', 'seguranca.usuarios.visualizar', true),
    ('81820602-0001-0000-0001-000000000002', v_recurso_seg_usuarios_id, 'Alterar',    'seguranca.usuarios.alterar',    true),
    ('81820602-0001-0000-0001-000000000003', v_recurso_seg_usuarios_id, 'Inativar',   'seguranca.usuarios.inativar',   true)
ON CONFLICT (codigo) DO UPDATE SET nome = EXCLUDED.nome;

-- Recurso: Perfis
INSERT INTO seguranca.recurso (public_id, modulo_id, nome, codigo, ativo, habilitado)
VALUES ('81820602-0000-0000-0002-000000000001', v_modulo_seguranca_id, 'Perfis', 'SEGURANCA_PERFIS', true, true)
ON CONFLICT (modulo_id, codigo) DO UPDATE SET nome = EXCLUDED.nome
RETURNING id INTO v_recurso_seg_perfis_id;

INSERT INTO seguranca.permissao (public_id, recurso_id, nome, codigo, ativo)
VALUES
    ('81820602-0002-0000-0001-000000000001', v_recurso_seg_perfis_id, 'Visualizar', 'seguranca.perfis.visualizar', true),
    ('81820602-0002-0000-0001-000000000002', v_recurso_seg_perfis_id, 'Inserir',    'seguranca.perfis.inserir',    true),
    ('81820602-0002-0000-0001-000000000003', v_recurso_seg_perfis_id, 'Alterar',    'seguranca.perfis.alterar',    true),
    ('81820602-0002-0000-0001-000000000004', v_recurso_seg_perfis_id, 'Excluir',    'seguranca.perfis.excluir',    true)
ON CONFLICT (codigo) DO UPDATE SET nome = EXCLUDED.nome;

-- Recurso: Catálogo
INSERT INTO seguranca.recurso (public_id, modulo_id, nome, codigo, ativo, habilitado)
VALUES ('81820602-0000-0000-0003-000000000001', v_modulo_seguranca_id, 'Catálogo', 'SEGURANCA_CATALOGO', true, true)
ON CONFLICT (modulo_id, codigo) DO UPDATE SET nome = EXCLUDED.nome
RETURNING id INTO v_recurso_seg_catalogo_id;

INSERT INTO seguranca.permissao (public_id, recurso_id, nome, codigo, ativo)
VALUES
    ('81820602-0003-0000-0001-000000000001', v_recurso_seg_catalogo_id, 'Visualizar', 'seguranca.catalogo.visualizar', true),
    ('81820602-0003-0000-0001-000000000002', v_recurso_seg_catalogo_id, 'Alterar',    'seguranca.catalogo.alterar',    true)
ON CONFLICT (codigo) DO UPDATE SET nome = EXCLUDED.nome;

-- Recurso: Auditoria
INSERT INTO seguranca.recurso (public_id, modulo_id, nome, codigo, ativo, habilitado)
VALUES ('81820602-0000-0000-0004-000000000001', v_modulo_seguranca_id, 'Auditoria', 'SEGURANCA_AUDITORIA', true, true)
ON CONFLICT (modulo_id, codigo) DO UPDATE SET nome = EXCLUDED.nome
RETURNING id INTO v_recurso_seg_auditoria_id;

INSERT INTO seguranca.permissao (public_id, recurso_id, nome, codigo, ativo)
VALUES
    ('81820602-0004-0000-0001-000000000001', v_recurso_seg_auditoria_id, 'Visualizar', 'seguranca.auditoria.visualizar', true)
ON CONFLICT (codigo) DO UPDATE SET nome = EXCLUDED.nome;

-- ============================================================
-- PERFIL: ADMINISTRADOR (sistema, acesso total)
-- ============================================================
INSERT INTO seguranca.perfil (public_id, codigo, nome, descricao, perfil_sistema, acesso_total, ativo)
VALUES (
    '81820603-0000-0000-0000-000000000001',
    'ADMINISTRADOR',
    'Administrador',
    'Acesso irrestrito a todos os módulos e recursos do sistema.',
    true,
    true,
    true
)
ON CONFLICT (codigo) DO UPDATE SET
    nome          = EXCLUDED.nome,
    acesso_total  = true,
    perfil_sistema = true,
    ativo         = true
RETURNING id INTO v_perfil_admin_id;

-- ============================================================
-- PERFIL: ADMINISTRATIVO (sistema, acesso restrito a clientes)
-- ============================================================
INSERT INTO seguranca.perfil (public_id, codigo, nome, descricao, perfil_sistema, acesso_total, ativo)
VALUES (
    '81820603-0000-0000-0000-000000000002',
    'ADMINISTRATIVO',
    'Administrativo',
    'Acesso ao módulo de Clientes e operações administrativas básicas.',
    true,
    false,
    true
)
ON CONFLICT (codigo) DO UPDATE SET
    nome          = EXCLUDED.nome,
    perfil_sistema = true,
    ativo         = true
RETURNING id INTO v_perfil_admtivo_id;

-- Permissões padrão do perfil ADMINISTRATIVO (Clientes completo)
INSERT INTO seguranca.perfil_permissao (perfil_id, permissao_id)
SELECT v_perfil_admtivo_id, id FROM seguranca.permissao
WHERE codigo IN (
    'clientes.visualizar', 'clientes.inserir', 'clientes.alterar',
    'clientes.inativar',   'clientes.reativar'
)
ON CONFLICT (perfil_id, permissao_id) DO NOTHING;

END $$;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM seguranca.perfil_permissao
WHERE perfil_id IN (
    SELECT id FROM seguranca.perfil WHERE codigo IN ('ADMINISTRATIVO')
);

DELETE FROM seguranca.perfil WHERE codigo IN ('ADMINISTRATIVO');
-- Nota: Não deletamos ADMINISTRADOR pois pode já existir com outros vínculos.

DELETE FROM seguranca.permissao WHERE codigo LIKE 'clientes.%';
DELETE FROM seguranca.permissao WHERE codigo LIKE 'estipulantes.%';
DELETE FROM seguranca.permissao WHERE codigo LIKE 'seguranca.%';

DELETE FROM seguranca.recurso WHERE codigo IN ('CLIENTES', 'ESTIPULANTES',
    'SEGURANCA_USUARIOS', 'SEGURANCA_PERFIS', 'SEGURANCA_CATALOGO', 'SEGURANCA_AUDITORIA');

DELETE FROM seguranca.modulo WHERE codigo IN ('CLIENTES', 'ESTIPULANTES', 'SEGURANCA');
            ");
        }
    }
}
