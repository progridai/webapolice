using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Seguranca.Migrations
{
    /// <inheritdoc />
    public partial class CargaAdministracaoSeguranca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var timestamp = "'2026-07-24 00:00:00Z'";

            migrationBuilder.Sql($@"
                INSERT INTO seguranca.modulo (public_id, codigo, nome, descricao, icone, ordem, ativo, created_at, updated_at)
                VALUES ('e1eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'SEGURANCA', 'Segurança', 'Administração de usuários, perfis e permissões do WebApólice', 'shield', 90, true, {timestamp}, {timestamp});
            ");

            migrationBuilder.Sql($@"
                INSERT INTO seguranca.recurso (public_id, modulo_id, codigo, nome, descricao, rota_frontend, ordem, ativo, created_at, updated_at)
                SELECT 'e2eebc99-9c0b-4ef8-bb6d-6bb9bd380a21', m.id, 'USUARIOS', 'Usuários', 'Gerenciamento de usuários e acessos', '/seguranca/usuarios', 10, true, {timestamp}, {timestamp}
                FROM seguranca.modulo m WHERE m.codigo = 'SEGURANCA';

                INSERT INTO seguranca.recurso (public_id, modulo_id, codigo, nome, descricao, rota_frontend, ordem, ativo, created_at, updated_at)
                SELECT 'e3eebc99-9c0b-4ef8-bb6d-6bb9bd380a22', m.id, 'PERFIS', 'Perfis de Acesso', 'Gerenciamento de perfis e permissões funcionais', '/seguranca/perfis', 20, true, {timestamp}, {timestamp}
                FROM seguranca.modulo m WHERE m.codigo = 'SEGURANCA';

                INSERT INTO seguranca.recurso (public_id, modulo_id, codigo, nome, descricao, rota_frontend, ordem, ativo, created_at, updated_at)
                SELECT 'e4eebc99-9c0b-4ef8-bb6d-6bb9bd380a23', m.id, 'CATALOGO', 'Catálogo Técnico', 'Visualização do catálogo de módulos e recursos do sistema', '/seguranca/catalogo', 30, true, {timestamp}, {timestamp}
                FROM seguranca.modulo m WHERE m.codigo = 'SEGURANCA';

                INSERT INTO seguranca.recurso (public_id, modulo_id, codigo, nome, descricao, rota_frontend, ordem, ativo, created_at, updated_at)
                SELECT 'e5eebc99-9c0b-4ef8-bb6d-6bb9bd380a24', m.id, 'AUDITORIA', 'Auditoria de Acesso', 'Histórico de alterações de segurança', '/seguranca/auditoria', 40, true, {timestamp}, {timestamp}
                FROM seguranca.modulo m WHERE m.codigo = 'SEGURANCA';
            ");

            migrationBuilder.Sql($@"
                INSERT INTO seguranca.permissao (public_id, recurso_id, codigo, nome, descricao, ativo, created_at, updated_at)
                SELECT 'f1eebc99-9c0b-4ef8-bb6d-6bb9bd380a31', r.id, 'seguranca.usuarios.visualizar', 'Visualizar usuários', 'Permite consultar a listagem e os detalhes dos usuários', true, {timestamp}, {timestamp}
                FROM seguranca.recurso r WHERE r.codigo = 'USUARIOS';

                INSERT INTO seguranca.permissao (public_id, recurso_id, codigo, nome, descricao, ativo, created_at, updated_at)
                SELECT 'f2eebc99-9c0b-4ef8-bb6d-6bb9bd380a32', r.id, 'seguranca.usuarios.inserir', 'Inserir usuários', 'Permite cadastrar novos usuários e definir senha temporária', true, {timestamp}, {timestamp}
                FROM seguranca.recurso r WHERE r.codigo = 'USUARIOS';

                INSERT INTO seguranca.permissao (public_id, recurso_id, codigo, nome, descricao, ativo, created_at, updated_at)
                SELECT 'f3eebc99-9c0b-4ef8-bb6d-6bb9bd380a33', r.id, 'seguranca.usuarios.alterar', 'Alterar usuários', 'Permite atualizar dados e o status dos usuários', true, {timestamp}, {timestamp}
                FROM seguranca.recurso r WHERE r.codigo = 'USUARIOS';

                INSERT INTO seguranca.permissao (public_id, recurso_id, codigo, nome, descricao, ativo, created_at, updated_at)
                SELECT 'f4eebc99-9c0b-4ef8-bb6d-6bb9bd380a34', r.id, 'seguranca.perfis.visualizar', 'Visualizar perfis', 'Permite consultar a listagem e detalhes dos perfis de acesso', true, {timestamp}, {timestamp}
                FROM seguranca.recurso r WHERE r.codigo = 'PERFIS';

                INSERT INTO seguranca.permissao (public_id, recurso_id, codigo, nome, descricao, ativo, created_at, updated_at)
                SELECT 'f5eebc99-9c0b-4ef8-bb6d-6bb9bd380a35', r.id, 'seguranca.perfis.inserir', 'Inserir perfis', 'Permite cadastrar novos perfis de acesso', true, {timestamp}, {timestamp}
                FROM seguranca.recurso r WHERE r.codigo = 'PERFIS';

                INSERT INTO seguranca.permissao (public_id, recurso_id, codigo, nome, descricao, ativo, created_at, updated_at)
                SELECT 'f6eebc99-9c0b-4ef8-bb6d-6bb9bd380a36', r.id, 'seguranca.perfis.alterar', 'Alterar perfis', 'Permite alterar permissões e dados dos perfis de acesso', true, {timestamp}, {timestamp}
                FROM seguranca.recurso r WHERE r.codigo = 'PERFIS';

                INSERT INTO seguranca.permissao (public_id, recurso_id, codigo, nome, descricao, ativo, created_at, updated_at)
                SELECT 'f7eebc99-9c0b-4ef8-bb6d-6bb9bd380a37', r.id, 'seguranca.catalogo.visualizar', 'Visualizar catálogo', 'Permite visualizar os módulos e recursos técnicos do sistema', true, {timestamp}, {timestamp}
                FROM seguranca.recurso r WHERE r.codigo = 'CATALOGO';

                INSERT INTO seguranca.permissao (public_id, recurso_id, codigo, nome, descricao, ativo, created_at, updated_at)
                SELECT 'f8eebc99-9c0b-4ef8-bb6d-6bb9bd380a38', r.id, 'seguranca.auditoria.visualizar', 'Visualizar auditoria', 'Permite consultar a auditoria de acessos e configurações de segurança', true, {timestamp}, {timestamp}
                FROM seguranca.recurso r WHERE r.codigo = 'AUDITORIA';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM seguranca.permissao WHERE codigo IN (
                    'seguranca.usuarios.visualizar', 'seguranca.usuarios.inserir', 'seguranca.usuarios.alterar',
                    'seguranca.perfis.visualizar', 'seguranca.perfis.inserir', 'seguranca.perfis.alterar',
                    'seguranca.catalogo.visualizar',
                    'seguranca.auditoria.visualizar'
                );
            ");

            migrationBuilder.Sql(@"
                DELETE FROM seguranca.recurso WHERE codigo IN ('USUARIOS', 'PERFIS', 'CATALOGO', 'AUDITORIA');
            ");

            migrationBuilder.Sql(@"
                DELETE FROM seguranca.modulo WHERE codigo = 'SEGURANCA';
            ");
        }
    }
}
