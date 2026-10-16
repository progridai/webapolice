DO $$
DECLARE r record; actual text[];
BEGIN
 FOR r IN SELECT * FROM (VALUES
 ('51-32121964',ARRAY['5132121964']),
 ('(53) 99711-9087',ARRAY['53997119087']),
 ('0055 32228896',ARRAY['5532228896']),
 ('+55 (53) 99711-9087',ARRAY['53997119087']),
 ('0055 53 99711-9087',ARRAY['53997119087']),
 ('5535651702',ARRAY['5535651702']),
 ('3459 2454',ARRAY[]::text[]),
 ('(048) 233065800000',ARRAY[]::text[]),
 ('937',ARRAY[]::text[]),
 ('51 92271327',ARRAY[]::text[]),
 ('54991390676 - FILHA CLECI',ARRAY[]::text[]),
 ('+51 997119087',ARRAY[]::text[]),
 ('0000 0',ARRAY[]::text[]),
 ('',ARRAY[]::text[]),
 ('5132336223 51-3227-2351',ARRAY[]::text[])
 ) v(input,expected)
 LOOP
 actual:=pg_temp.telefone_br_candidatos(r.input);
 IF actual IS DISTINCT FROM r.expected THEN RAISE EXCEPTION 'Falha teste: %, esperado %, obtido %',r.input,r.expected,actual; END IF;
 END LOOP;
END $$;
SELECT '15 testes de classificacao aprovados' testes;
