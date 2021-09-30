CREATE OR REPLACE FUNCTION "@get_id"(p_schema_id bigint, p_table_name text) RETURNS bigint AS $$
    declare
		lv_result bigint;
		lv_id bigint;
	    lv_table_name text := lower(p_table_name);
    begin
		select id into lv_id from "@meta" where schema_id=p_schema_id and object_type=0 and lower(name)=lv_table_name;
		UPDATE "@meta_id" SET value=value+1 WHERE id=lv_id AND schema_id=p_schema_id RETURNING value into lv_result;
		/** reserve id==0*/
	    IF lv_result=1 then 
	   		UPDATE "@meta_id" SET value=value+1 WHERE id=lv_id AND schema_id=p_schema_id RETURNING value into lv_result;
	   	end IF; 
    	RETURN lv_result-1;
    END;
$$ LANGUAGE plpgsql;
