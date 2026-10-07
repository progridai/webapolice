START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguranca."__EFMigrationsHistory" WHERE "migration_id" = '20261006030000_CatalogosConveniosCoberturasPlanos') THEN

    DO $$ BEGIN
     IF NOT EXISTS(SELECT 1 FROM seguranca.modulo WHERE codigo='CADASTRO') OR NOT EXISTS(SELECT 1 FROM seguranca.recurso WHERE codigo='APOLICES') THEN
      RAISE EXCEPTION 'Catálogo exige módulo CADASTRO e recurso APOLICES existentes';
     END IF;
    END $$;
    INSERT INTO seguranca.recurso(public_id,modulo_id,nome,codigo,habilitado,ativo,rota_frontend) SELECT '814d5241-9d2f-4352-872a-756033a7a5d6'::uuid,id,'Convênios de Cobrança','CONVENIOS_COBRANCA',true,true,'/convenios-cobranca' FROM seguranca.modulo WHERE codigo='CADASTRO' ON CONFLICT(modulo_id,codigo) DO NOTHING;
    INSERT INTO seguranca.permissao(public_id,recurso_id,nome,codigo,ativo) SELECT 'e2a9e06a-3c9e-4e81-95ee-dadcbcc7c32d'::uuid,id,'Visualizar Convênios de Cobrança','convenios_cobranca.visualizar',true FROM seguranca.recurso WHERE codigo='CONVENIOS_COBRANCA' AND modulo_id=(SELECT id FROM seguranca.modulo WHERE codigo='CADASTRO') ON CONFLICT(codigo) DO NOTHING;
    INSERT INTO seguranca.permissao(public_id,recurso_id,nome,codigo,ativo) SELECT '3fc647d8-2add-46ab-a9e8-7dce477d1f2a'::uuid,id,'Inserir Convênios de Cobrança','convenios_cobranca.inserir',true FROM seguranca.recurso WHERE codigo='CONVENIOS_COBRANCA' AND modulo_id=(SELECT id FROM seguranca.modulo WHERE codigo='CADASTRO') ON CONFLICT(codigo) DO NOTHING;
    INSERT INTO seguranca.permissao(public_id,recurso_id,nome,codigo,ativo) SELECT 'db06ef72-5f88-45ce-a88a-879b08b91be3'::uuid,id,'Alterar Convênios de Cobrança','convenios_cobranca.alterar',true FROM seguranca.recurso WHERE codigo='CONVENIOS_COBRANCA' AND modulo_id=(SELECT id FROM seguranca.modulo WHERE codigo='CADASTRO') ON CONFLICT(codigo) DO NOTHING;
    INSERT INTO seguranca.permissao(public_id,recurso_id,nome,codigo,ativo) SELECT 'b00d9168-73ba-45a9-83ae-c53cc51d93a8'::uuid,id,'Inativar Convênios de Cobrança','convenios_cobranca.inativar',true FROM seguranca.recurso WHERE codigo='CONVENIOS_COBRANCA' AND modulo_id=(SELECT id FROM seguranca.modulo WHERE codigo='CADASTRO') ON CONFLICT(codigo) DO NOTHING;
    INSERT INTO seguranca.permissao(public_id,recurso_id,nome,codigo,ativo) SELECT 'b6737244-41e8-4a20-8b97-67b4d133e6b5'::uuid,id,'Reativar Convênios de Cobrança','convenios_cobranca.reativar',true FROM seguranca.recurso WHERE codigo='CONVENIOS_COBRANCA' AND modulo_id=(SELECT id FROM seguranca.modulo WHERE codigo='CADASTRO') ON CONFLICT(codigo) DO NOTHING;
    INSERT INTO seguranca.recurso(public_id,modulo_id,nome,codigo,habilitado,ativo,rota_frontend) SELECT 'd4def7c6-2a5e-442b-bddf-24c161783d70'::uuid,id,'Coberturas','COBERTURAS',true,true,'/coberturas' FROM seguranca.modulo WHERE codigo='CADASTRO' ON CONFLICT(modulo_id,codigo) DO NOTHING;
    INSERT INTO seguranca.permissao(public_id,recurso_id,nome,codigo,ativo) SELECT 'f231aeec-0603-434c-8c6e-4d2e03a9db33'::uuid,id,'Visualizar Coberturas','coberturas.visualizar',true FROM seguranca.recurso WHERE codigo='COBERTURAS' AND modulo_id=(SELECT id FROM seguranca.modulo WHERE codigo='CADASTRO') ON CONFLICT(codigo) DO NOTHING;
    INSERT INTO seguranca.permissao(public_id,recurso_id,nome,codigo,ativo) SELECT '70394129-8148-4722-8fd9-c0330ae0a233'::uuid,id,'Inserir Coberturas','coberturas.inserir',true FROM seguranca.recurso WHERE codigo='COBERTURAS' AND modulo_id=(SELECT id FROM seguranca.modulo WHERE codigo='CADASTRO') ON CONFLICT(codigo) DO NOTHING;
    INSERT INTO seguranca.permissao(public_id,recurso_id,nome,codigo,ativo) SELECT '3bb05f8c-9744-480c-8e51-7108e1f7bf86'::uuid,id,'Alterar Coberturas','coberturas.alterar',true FROM seguranca.recurso WHERE codigo='COBERTURAS' AND modulo_id=(SELECT id FROM seguranca.modulo WHERE codigo='CADASTRO') ON CONFLICT(codigo) DO NOTHING;
    INSERT INTO seguranca.permissao(public_id,recurso_id,nome,codigo,ativo) SELECT '227e3479-467e-4469-91dc-fda359ba48fd'::uuid,id,'Inativar Coberturas','coberturas.inativar',true FROM seguranca.recurso WHERE codigo='COBERTURAS' AND modulo_id=(SELECT id FROM seguranca.modulo WHERE codigo='CADASTRO') ON CONFLICT(codigo) DO NOTHING;
    INSERT INTO seguranca.permissao(public_id,recurso_id,nome,codigo,ativo) SELECT '46b8328d-ccec-4106-80b0-3595fbe6473a'::uuid,id,'Reativar Coberturas','coberturas.reativar',true FROM seguranca.recurso WHERE codigo='COBERTURAS' AND modulo_id=(SELECT id FROM seguranca.modulo WHERE codigo='CADASTRO') ON CONFLICT(codigo) DO NOTHING;
    INSERT INTO seguranca.recurso(public_id,modulo_id,nome,codigo,habilitado,ativo,rota_frontend) SELECT '222918b0-2c72-412d-ab87-2894f2b04313'::uuid,id,'Planos','PLANOS',true,true,'/planos' FROM seguranca.modulo WHERE codigo='CADASTRO' ON CONFLICT(modulo_id,codigo) DO NOTHING;
    INSERT INTO seguranca.permissao(public_id,recurso_id,nome,codigo,ativo) SELECT 'f8facca7-8c0d-4010-a5c2-e4159e29daa6'::uuid,id,'Visualizar Planos','planos.visualizar',true FROM seguranca.recurso WHERE codigo='PLANOS' AND modulo_id=(SELECT id FROM seguranca.modulo WHERE codigo='CADASTRO') ON CONFLICT(codigo) DO NOTHING;
    INSERT INTO seguranca.permissao(public_id,recurso_id,nome,codigo,ativo) SELECT '70b055d1-8629-4ae2-bdce-66bf9dc15383'::uuid,id,'Inserir Planos','planos.inserir',true FROM seguranca.recurso WHERE codigo='PLANOS' AND modulo_id=(SELECT id FROM seguranca.modulo WHERE codigo='CADASTRO') ON CONFLICT(codigo) DO NOTHING;
    INSERT INTO seguranca.permissao(public_id,recurso_id,nome,codigo,ativo) SELECT 'ff3824c4-2e1e-4064-9901-1fc37f10cf1b'::uuid,id,'Alterar Planos','planos.alterar',true FROM seguranca.recurso WHERE codigo='PLANOS' AND modulo_id=(SELECT id FROM seguranca.modulo WHERE codigo='CADASTRO') ON CONFLICT(codigo) DO NOTHING;
    INSERT INTO seguranca.permissao(public_id,recurso_id,nome,codigo,ativo) SELECT 'fb9db0af-d38b-4a25-bf0c-d7985af7424d'::uuid,id,'Inativar Planos','planos.inativar',true FROM seguranca.recurso WHERE codigo='PLANOS' AND modulo_id=(SELECT id FROM seguranca.modulo WHERE codigo='CADASTRO') ON CONFLICT(codigo) DO NOTHING;
    INSERT INTO seguranca.permissao(public_id,recurso_id,nome,codigo,ativo) SELECT '822f2f44-b180-49d2-a6c9-1b81b1d017d2'::uuid,id,'Reativar Planos','planos.reativar',true FROM seguranca.recurso WHERE codigo='PLANOS' AND modulo_id=(SELECT id FROM seguranca.modulo WHERE codigo='CADASTRO') ON CONFLICT(codigo) DO NOTHING;
    INSERT INTO seguranca.permissao(public_id,recurso_id,nome,codigo,ativo) SELECT '5c60143c-2542-4ae7-961c-92938aa8d87a'::uuid,id,'Gerenciar Coberturas do Plano','planos.coberturas.alterar',true FROM seguranca.recurso WHERE codigo='PLANOS' AND modulo_id=(SELECT id FROM seguranca.modulo WHERE codigo='CADASTRO') ON CONFLICT(codigo) DO NOTHING;
    INSERT INTO seguranca.permissao(public_id,recurso_id,nome,codigo,ativo) SELECT '81d1b3d1-eba7-4705-a662-03bfce8b1b08'::uuid,id,'Inserir Subgrupo da Apólice','apolices.subgrupos.inserir',true FROM seguranca.recurso WHERE codigo='APOLICES' ON CONFLICT(codigo) DO NOTHING;
    INSERT INTO seguranca.permissao(public_id,recurso_id,nome,codigo,ativo) SELECT '93ca9ebf-9b76-4aac-bb88-bce6e2a906e4'::uuid,id,'Alterar Subgrupo da Apólice','apolices.subgrupos.alterar',true FROM seguranca.recurso WHERE codigo='APOLICES' ON CONFLICT(codigo) DO NOTHING;
    INSERT INTO seguranca.permissao(public_id,recurso_id,nome,codigo,ativo) SELECT '293c27ee-aba9-45a3-a42d-a4e1bc0f5a83'::uuid,id,'Inativar Subgrupo da Apólice','apolices.subgrupos.inativar',true FROM seguranca.recurso WHERE codigo='APOLICES' ON CONFLICT(codigo) DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguranca."__EFMigrationsHistory" WHERE "migration_id" = '20261006030000_CatalogosConveniosCoberturasPlanos') THEN
    INSERT INTO seguranca."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20261006030000_CatalogosConveniosCoberturasPlanos', '10.0.11');
    END IF;
END $EF$;
COMMIT;
