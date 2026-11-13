BEGIN READ ONLY;
SET LOCAL statement_timeout = '15s';
SELECT current_database() AS banco;
SELECT id, codigo, nome, permite_movimentacao, visivel_operacional, finalizador
FROM seguro.proposta_status ORDER BY id;
SELECT legado_id, nome, gera_titulo, classificacao, ativo, altera_proposta, financeiro, cancelamento
FROM seguro.movimento_tipo ORDER BY legado_id, id;
SELECT 'seguro' AS contexto, migration_id FROM seguro."__EFMigrationsHistory"
WHERE migration_id IN ('20261023000100_AdicionarPublicIdPropostaItem', '20261109232500_ProtegerVinculosVidas', '20261111213321_ExclusaoHierarquicaVinculosApolice', '20261111224713_PropostasOperacionaisModulo', '20261111224800_DefinirRascunhoProposta')
UNION ALL
SELECT 'seguranca', migration_id FROM seguranca."__EFMigrationsHistory"
WHERE migration_id = '20261023000200_CatalogarPropostas';
SELECT table_name, column_name, is_nullable, data_type FROM information_schema.columns
WHERE table_schema = 'seguro' AND
((table_name = 'proposta_item' AND column_name IN ('public_id', 'legado_id', 'plano_id')) OR
(table_name IN ('proposta_cobertura', 'proposta_beneficiario') AND column_name IN ('public_id', 'legado_id')))
ORDER BY table_name, column_name;
SELECT codigo, ativo FROM seguranca.permissao WHERE codigo LIKE 'propostas.%' ORDER BY codigo;
SELECT COUNT(*) AS propostas, COUNT(*) FILTER (WHERE apolice_vida_id IS NOT NULL) AS com_vida FROM seguro.proposta;
ROLLBACK;
