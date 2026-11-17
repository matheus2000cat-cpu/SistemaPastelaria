ALTER TABLE public.caixa ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.pedido ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.item_pedido ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS "app_read_caixa" ON public.caixa;
CREATE POLICY "app_read_caixa"
    ON public.caixa FOR SELECT TO anon, authenticated
    USING (TRUE);

DROP POLICY IF EXISTS "app_insert_caixa" ON public.caixa;
CREATE POLICY "app_insert_caixa"
    ON public.caixa FOR INSERT TO anon, authenticated
    WITH CHECK (valor_inicial >= 0 AND status = 'Aberto');

DROP POLICY IF EXISTS "app_update_caixa" ON public.caixa;
CREATE POLICY "app_update_caixa"
    ON public.caixa FOR UPDATE TO anon, authenticated
    USING (TRUE)
    WITH CHECK (status IN ('Aberto', 'Fechado'));

DROP POLICY IF EXISTS "app_read_pedidos" ON public.pedido;
CREATE POLICY "app_read_pedidos"
    ON public.pedido FOR SELECT TO anon, authenticated
    USING (TRUE);

DROP POLICY IF EXISTS "app_insert_pedidos" ON public.pedido;
CREATE POLICY "app_insert_pedidos"
    ON public.pedido FOR INSERT TO anon, authenticated
    WITH CHECK (valor_total >= 0);

DROP POLICY IF EXISTS "app_update_pedidos" ON public.pedido;
CREATE POLICY "app_update_pedidos"
    ON public.pedido FOR UPDATE TO anon, authenticated
    USING (TRUE)
    WITH CHECK (valor_total >= 0);

DROP POLICY IF EXISTS "app_read_itens_pedido" ON public.item_pedido;
CREATE POLICY "app_read_itens_pedido"
    ON public.item_pedido FOR SELECT TO anon, authenticated
    USING (TRUE);

DROP POLICY IF EXISTS "app_insert_itens_pedido" ON public.item_pedido;
CREATE POLICY "app_insert_itens_pedido"
    ON public.item_pedido FOR INSERT TO anon, authenticated
    WITH CHECK (quantidade > 0 AND preco_unitario >= 0);
