SELECT current_database() banco, current_user usuario;
SELECT tipo_contato, ativo, count(*) quantidade FROM core.pessoa_contato GROUP BY 1,2 ORDER BY 1,2;
SELECT conname, pg_get_constraintdef(oid) definicao FROM pg_constraint WHERE conrelid='core.pessoa_contato'::regclass;
SELECT column_name,data_type FROM information_schema.columns WHERE table_schema='core' AND table_name IN ('pessoa_contato','pessoa_endereco') ORDER BY table_name,ordinal_position;
SELECT indexdef FROM pg_indexes WHERE schemaname='core' AND tablename='pessoa_contato';
SELECT length(regexp_replace(valor,'[^0-9]','','g')) digitos,count(*) quantidade FROM core.pessoa_contato c WHERE ativo AND tipo_contato<>'EMAIL' AND EXISTS(SELECT 1 FROM cadastro.cliente cl WHERE cl.pessoa_id=c.pessoa_id) GROUP BY 1 ORDER BY 1;
