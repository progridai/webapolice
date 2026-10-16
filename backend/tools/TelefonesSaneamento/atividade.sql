SELECT pid,state,wait_event_type,wait_event,now()-query_start duracao,left(query,100) comando FROM pg_stat_activity WHERE datname=current_database() AND state<>'idle';
