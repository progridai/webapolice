using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Seguranca.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarRecursoModulosCadastro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                DECLARE
                    v_modulo_id bigint;
                    v_recurso_id bigint;
                    v_perfil_admin_id bigint;
                BEGIN
                    -- 1. Obter o ID do módulo CADASTRO
                    SELECT id INTO v_modulo_id FROM seguranca.modulo WHERE codigo = 'CADASTRO';

                    -- 2. Obter o ID do perfil ADMINISTRATIVO
                    SELECT id INTO v_perfil_admin_id FROM seguranca.perfil WHERE codigo = 'ADMINISTRATIVO';

                    -- 3. Inserir o Recurso 'Módulos Globais'
                    INSERT INTO seguranca.recurso (modulo_id, codigo, nome, ativo, created_at, updated_at)
                    VALUES (v_modulo_id, 'MODULOS', 'Módulos Globais', true, now(), now())
                    RETURNING id INTO v_recurso_id;

                    -- 4. Inserir as Permissões do Recurso
                    INSERT INTO seguranca.permissao (recurso_id, codigo, nome, ativo, created_at, updated_at) VALUES 
                        (v_recurso_id, 'modulos.visualizar', 'Visualizar', true, now(), now()),
                        (v_recurso_id, 'modulos.inserir', 'Inserir', true, now(), now()),
                        (v_recurso_id, 'modulos.alterar', 'Alterar', true, now(), now()),
                        (v_recurso_id, 'modulos.inativar', 'Inativar', true, now(), now());

                    -- 5. Atribuir todas as permissões ao perfil ADMINISTRATIVO
                    INSERT INTO seguranca.perfil_permissao (perfil_id, permissao_id)
                    SELECT v_perfil_admin_id, p.id
                    FROM seguranca.permissao p
                    WHERE p.recurso_id = v_recurso_id;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                DECLARE
                    v_recurso_id bigint;
                BEGIN
                    SELECT id INTO v_recurso_id FROM seguranca.recurso WHERE codigo = 'MODULOS';
                    
                    IF v_recurso_id IS NOT NULL THEN
                        DELETE FROM seguranca.perfil_permissao WHERE permissao_id IN (SELECT id FROM seguranca.permissao WHERE recurso_id = v_recurso_id);
                        DELETE FROM seguranca.permissao WHERE recurso_id = v_recurso_id;
                        DELETE FROM seguranca.recurso WHERE id = v_recurso_id;
                    END IF;
                END $$;
            ");
        }
    }
}
