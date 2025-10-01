--
-- PostgreSQL database dump
--

-- Dumped from database version 17.2
-- Dumped by pg_dump version 17.2

-- Started on 2025-09-25 12:27:49


-- TOC entry 232 (class 1255 OID 28132)
-- Name: calcular_subtotal_y_total(integer, character varying, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.calcular_subtotal_y_total(p_fknumfactura integer, p_fkcodproducto character varying, p_cantidad integer) RETURNS TABLE(fknumfactura integer, fkcodproducto character varying, cantidad integer, subtotal double precision, insuficiente_stock boolean)
    LANGUAGE plpgsql
    AS $$
BEGIN
    RETURN QUERY
    SELECT 
        p_fknumfactura,
        p_fkcodproducto,
        p_cantidad,
        p_cantidad * p.valorunitario,
        CASE 
            WHEN p_cantidad > p.stock THEN TRUE
            ELSE FALSE
        END
    FROM producto p
    WHERE p.codigo = p_fkcodproducto;
END;
$$;


ALTER FUNCTION public.calcular_subtotal_y_total(p_fknumfactura integer, p_fkcodproducto character varying, p_cantidad integer) OWNER TO postgres;

--
-- TOC entry 233 (class 1255 OID 28133)
-- Name: trigger_calcular_subtotal_y_total(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.trigger_calcular_subtotal_y_total() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    DECLARE
        insuficiente_stock BOOLEAN;
    BEGIN
        SELECT MAX(insuficiente_stock)
        INTO insuficiente_stock
        FROM calcular_subtotal_y_total(NEW.fknumfactura, NEW.fkcodproducto, NEW.cantidad);
        
        IF insuficiente_stock THEN
            RAISE EXCEPTION 'No hay suficiente stock para el producto %', NEW.fkcodproducto;
        END IF;

        -- Actualizar el subtotal en la tabla productosporfactura
        UPDATE productosporfactura
        SET subtotal = NEW.cantidad * (SELECT valorunitario FROM producto WHERE codigo = NEW.fkcodproducto)
        WHERE fknumfactura = NEW.fknumfactura AND fkcodproducto = NEW.fkcodproducto;

        -- Actualizar el stock en la tabla producto
        UPDATE producto
        SET stock = stock - NEW.cantidad
        WHERE codigo = NEW.fkcodproducto;

        -- Actualizar el total de la factura
        UPDATE factura
        SET total = total + (NEW.cantidad * (SELECT valorunitario FROM producto WHERE codigo = NEW.fkcodproducto))
        WHERE numero = NEW.fknumfactura;

        RETURN NEW;
    END;
END;
$$;


ALTER FUNCTION public.trigger_calcular_subtotal_y_total() OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 226 (class 1259 OID 28073)
-- Name: cliente; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cliente (
    id integer NOT NULL,
    credito double precision DEFAULT 0 NOT NULL,
    fkcodpersona character varying(20) NOT NULL,
    fkcodempresa character varying(10)
);


ALTER TABLE public.cliente OWNER TO postgres;

--
-- TOC entry 225 (class 1259 OID 28072)
-- Name: cliente_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.cliente_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.cliente_id_seq OWNER TO postgres;

--
-- TOC entry 4994 (class 0 OID 0)
-- Dependencies: 225
-- Name: cliente_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.cliente_id_seq OWNED BY public.cliente.id;


--
-- TOC entry 218 (class 1259 OID 28026)
-- Name: empresa; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.empresa (
    codigo character varying(10) NOT NULL,
    nombre character varying(200) NOT NULL
);


ALTER TABLE public.empresa OWNER TO postgres;

--
-- TOC entry 228 (class 1259 OID 28093)
-- Name: factura; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.factura (
    numero integer NOT NULL,
    fecha timestamp without time zone DEFAULT now() NOT NULL,
    total double precision DEFAULT 0 NOT NULL,
    fkidcliente integer NOT NULL,
    fkidvendedor integer NOT NULL
);


ALTER TABLE public.factura OWNER TO postgres;

--
-- TOC entry 227 (class 1259 OID 28092)
-- Name: factura_numero_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.factura_numero_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.factura_numero_seq OWNER TO postgres;

--
-- TOC entry 4995 (class 0 OID 0)
-- Dependencies: 227
-- Name: factura_numero_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.factura_numero_seq OWNED BY public.factura.numero;


--
-- TOC entry 217 (class 1259 OID 28021)
-- Name: persona; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.persona (
    codigo character varying(20) NOT NULL,
    nombre character varying(100) NOT NULL,
    email character varying(100) NOT NULL,
    telefono character varying(20) NOT NULL
);


ALTER TABLE public.persona OWNER TO postgres;

--
-- TOC entry 229 (class 1259 OID 28111)
-- Name: producto; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.producto (
    codigo character varying(30) NOT NULL,
    nombre character varying(100) NOT NULL,
    stock integer NOT NULL,
    valorunitario double precision NOT NULL
);


ALTER TABLE public.producto OWNER TO postgres;

--
-- TOC entry 230 (class 1259 OID 28116)
-- Name: productosporfactura; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.productosporfactura (
    fknumfactura integer NOT NULL,
    fkcodproducto character varying(30) NOT NULL,
    cantidad integer NOT NULL,
    subtotal double precision DEFAULT 0 NOT NULL
);


ALTER TABLE public.productosporfactura OWNER TO postgres;

--
-- TOC entry 221 (class 1259 OID 28037)
-- Name: rol; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.rol (
    id integer NOT NULL,
    nombre character varying(100) NOT NULL
);


ALTER TABLE public.rol OWNER TO postgres;

--
-- TOC entry 220 (class 1259 OID 28036)
-- Name: rol_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.rol_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.rol_id_seq OWNER TO postgres;

--
-- TOC entry 4996 (class 0 OID 0)
-- Dependencies: 220
-- Name: rol_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.rol_id_seq OWNED BY public.rol.id;


--
-- TOC entry 222 (class 1259 OID 28043)
-- Name: rol_usuario; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.rol_usuario (
    fkemail character varying(100) NOT NULL,
    fkidrol integer NOT NULL
);


ALTER TABLE public.rol_usuario OWNER TO postgres;

--
-- TOC entry 231 (class 1259 OID 28135)
-- Name: rutarol; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.rutarol (
    ruta character varying(4000) NOT NULL,
    rol character varying(100) NOT NULL
);


ALTER TABLE public.rutarol OWNER TO postgres;

--
-- TOC entry 219 (class 1259 OID 28031)
-- Name: usuario; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.usuario (
    email character varying(100) NOT NULL,
    contrasena character varying(100) NOT NULL
);


ALTER TABLE public.usuario OWNER TO postgres;

--
-- TOC entry 224 (class 1259 OID 28059)
-- Name: vendedor; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.vendedor (
    id integer NOT NULL,
    carnet integer NOT NULL,
    direccion character varying(100) NOT NULL,
    fkcodpersona character varying(20) NOT NULL
);


ALTER TABLE public.vendedor OWNER TO postgres;

--
-- TOC entry 223 (class 1259 OID 28058)
-- Name: vendedor_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.vendedor_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.vendedor_id_seq OWNER TO postgres;

--
-- TOC entry 4997 (class 0 OID 0)
-- Dependencies: 223
-- Name: vendedor_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.vendedor_id_seq OWNED BY public.vendedor.id;


--
-- TOC entry 4789 (class 2604 OID 28076)
-- Name: cliente id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cliente ALTER COLUMN id SET DEFAULT nextval('public.cliente_id_seq'::regclass);


--
-- TOC entry 4791 (class 2604 OID 28096)
-- Name: factura numero; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.factura ALTER COLUMN numero SET DEFAULT nextval('public.factura_numero_seq'::regclass);


--
-- TOC entry 4787 (class 2604 OID 28040)
-- Name: rol id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.rol ALTER COLUMN id SET DEFAULT nextval('public.rol_id_seq'::regclass);


--
-- TOC entry 4788 (class 2604 OID 28062)
-- Name: vendedor id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.vendedor ALTER COLUMN id SET DEFAULT nextval('public.vendedor_id_seq'::regclass);


--
-- TOC entry 4983 (class 0 OID 28073)
-- Dependencies: 226
-- Data for Name: cliente; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.cliente VALUES (1, 5000, 'P003', 'E001');
INSERT INTO public.cliente VALUES (2, 10000, 'P002', 'E002');


--
-- TOC entry 4975 (class 0 OID 28026)
-- Dependencies: 218
-- Data for Name: empresa; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.empresa VALUES ('E001', 'Empresa ABC');
INSERT INTO public.empresa VALUES ('E002', 'Empresa XYZ');


--
-- TOC entry 4985 (class 0 OID 28093)
-- Dependencies: 228
-- Data for Name: factura; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- TOC entry 4974 (class 0 OID 28021)
-- Dependencies: 217
-- Data for Name: persona; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.persona VALUES ('P001', 'Juan Pérez', 'juan.perez@email.com', '555-1234');
INSERT INTO public.persona VALUES ('P002', 'Ana Gómez', 'ana.gomez@email.com', '555-5678');
INSERT INTO public.persona VALUES ('P003', 'Carlos Ruiz', 'carlos.ruiz@email.com', '555-8765');
INSERT INTO public.persona VALUES ('P2', 'Ana López', 'ana@example.com', '987654321');


--
-- TOC entry 4986 (class 0 OID 28111)
-- Dependencies: 229
-- Data for Name: producto; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.producto VALUES ('PRD001', 'Producto A', 50, 100);
INSERT INTO public.producto VALUES ('PRD002', 'Producto B', 30, 200);
INSERT INTO public.producto VALUES ('PRD003', 'Producto C', 20, 150);
INSERT INTO public.producto VALUES ('PRD004', 'Producto D', 25, 120);
INSERT INTO public.producto VALUES ('PRD005', 'Producto E', 10, 180);


--
-- TOC entry 4987 (class 0 OID 28116)
-- Dependencies: 230
-- Data for Name: productosporfactura; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- TOC entry 4978 (class 0 OID 28037)
-- Dependencies: 221
-- Data for Name: rol; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.rol VALUES (1, 'Administrador');
INSERT INTO public.rol VALUES (2, 'Vendedor');


--
-- TOC entry 4979 (class 0 OID 28043)
-- Dependencies: 222
-- Data for Name: rol_usuario; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.rol_usuario VALUES ('admin@email.com', 1);
INSERT INTO public.rol_usuario VALUES ('vendedor@email.com', 2);


--
-- TOC entry 4988 (class 0 OID 28135)
-- Dependencies: 231
-- Data for Name: rutarol; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.rutarol VALUES ('/rol', 'Administrador');
INSERT INTO public.rutarol VALUES ('/usuario', 'Administrador');
INSERT INTO public.rutarol VALUES ('/persona', 'Administrador');
INSERT INTO public.rutarol VALUES ('/vendedor', 'Vendedor');


--
-- TOC entry 4976 (class 0 OID 28031)
-- Dependencies: 219
-- Data for Name: usuario; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.usuario VALUES ('admin@email.com', 'admin123');
INSERT INTO public.usuario VALUES ('vendedor@email.com', 'vendedor123');


--
-- TOC entry 4981 (class 0 OID 28059)
-- Dependencies: 224
-- Data for Name: vendedor; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.vendedor VALUES (1, 1001, 'Calle 123', 'P001');
INSERT INTO public.vendedor VALUES (2, 1002, 'Avenida 456', 'P002');


--
-- TOC entry 4998 (class 0 OID 0)
-- Dependencies: 225
-- Name: cliente_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.cliente_id_seq', 2, true);


--
-- TOC entry 4999 (class 0 OID 0)
-- Dependencies: 227
-- Name: factura_numero_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.factura_numero_seq', 1, false);


--
-- TOC entry 5000 (class 0 OID 0)
-- Dependencies: 220
-- Name: rol_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.rol_id_seq', 2, true);


--
-- TOC entry 5001 (class 0 OID 0)
-- Dependencies: 223
-- Name: vendedor_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.vendedor_id_seq', 2, true);


--
-- TOC entry 4810 (class 2606 OID 28081)
-- Name: cliente cliente_fkcodpersona_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cliente
    ADD CONSTRAINT cliente_fkcodpersona_key UNIQUE (fkcodpersona);


--
-- TOC entry 4812 (class 2606 OID 28079)
-- Name: cliente cliente_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cliente
    ADD CONSTRAINT cliente_pkey PRIMARY KEY (id);


--
-- TOC entry 4798 (class 2606 OID 28030)
-- Name: empresa empresa_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.empresa
    ADD CONSTRAINT empresa_pkey PRIMARY KEY (codigo);


--
-- TOC entry 4814 (class 2606 OID 28100)
-- Name: factura factura_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.factura
    ADD CONSTRAINT factura_pkey PRIMARY KEY (numero);


--
-- TOC entry 4796 (class 2606 OID 28025)
-- Name: persona persona_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.persona
    ADD CONSTRAINT persona_pkey PRIMARY KEY (codigo);


--
-- TOC entry 4816 (class 2606 OID 28115)
-- Name: producto producto_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.producto
    ADD CONSTRAINT producto_pkey PRIMARY KEY (codigo);


--
-- TOC entry 4818 (class 2606 OID 28121)
-- Name: productosporfactura productosporfactura_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.productosporfactura
    ADD CONSTRAINT productosporfactura_pkey PRIMARY KEY (fknumfactura, fkcodproducto);


--
-- TOC entry 4802 (class 2606 OID 28042)
-- Name: rol rol_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.rol
    ADD CONSTRAINT rol_pkey PRIMARY KEY (id);


--
-- TOC entry 4804 (class 2606 OID 28047)
-- Name: rol_usuario rol_usuario_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.rol_usuario
    ADD CONSTRAINT rol_usuario_pkey PRIMARY KEY (fkemail, fkidrol);


--
-- TOC entry 4800 (class 2606 OID 28035)
-- Name: usuario usuario_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.usuario
    ADD CONSTRAINT usuario_pkey PRIMARY KEY (email);


--
-- TOC entry 4806 (class 2606 OID 28066)
-- Name: vendedor vendedor_fkcodpersona_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.vendedor
    ADD CONSTRAINT vendedor_fkcodpersona_key UNIQUE (fkcodpersona);


--
-- TOC entry 4808 (class 2606 OID 28064)
-- Name: vendedor vendedor_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.vendedor
    ADD CONSTRAINT vendedor_pkey PRIMARY KEY (id);


--
-- TOC entry 4828 (class 2620 OID 28134)
-- Name: productosporfactura trigger_calcular_subtotal; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trigger_calcular_subtotal AFTER INSERT ON public.productosporfactura FOR EACH ROW EXECUTE FUNCTION public.trigger_calcular_subtotal_y_total();


--
-- TOC entry 4822 (class 2606 OID 28087)
-- Name: cliente cliente_fkcodempresa_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cliente
    ADD CONSTRAINT cliente_fkcodempresa_fkey FOREIGN KEY (fkcodempresa) REFERENCES public.empresa(codigo);


--
-- TOC entry 4823 (class 2606 OID 28082)
-- Name: cliente cliente_fkcodpersona_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cliente
    ADD CONSTRAINT cliente_fkcodpersona_fkey FOREIGN KEY (fkcodpersona) REFERENCES public.persona(codigo);


--
-- TOC entry 4824 (class 2606 OID 28101)
-- Name: factura factura_fkidcliente_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.factura
    ADD CONSTRAINT factura_fkidcliente_fkey FOREIGN KEY (fkidcliente) REFERENCES public.cliente(id);


--
-- TOC entry 4825 (class 2606 OID 28106)
-- Name: factura factura_fkidvendedor_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.factura
    ADD CONSTRAINT factura_fkidvendedor_fkey FOREIGN KEY (fkidvendedor) REFERENCES public.vendedor(id);


--
-- TOC entry 4826 (class 2606 OID 28127)
-- Name: productosporfactura productosporfactura_fkcodproducto_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.productosporfactura
    ADD CONSTRAINT productosporfactura_fkcodproducto_fkey FOREIGN KEY (fkcodproducto) REFERENCES public.producto(codigo);


--
-- TOC entry 4827 (class 2606 OID 28122)
-- Name: productosporfactura productosporfactura_fknumfactura_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.productosporfactura
    ADD CONSTRAINT productosporfactura_fknumfactura_fkey FOREIGN KEY (fknumfactura) REFERENCES public.factura(numero) ON DELETE CASCADE;


--
-- TOC entry 4819 (class 2606 OID 28048)
-- Name: rol_usuario rol_usuario_fkemail_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.rol_usuario
    ADD CONSTRAINT rol_usuario_fkemail_fkey FOREIGN KEY (fkemail) REFERENCES public.usuario(email) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 4820 (class 2606 OID 28053)
-- Name: rol_usuario rol_usuario_fkidrol_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.rol_usuario
    ADD CONSTRAINT rol_usuario_fkidrol_fkey FOREIGN KEY (fkidrol) REFERENCES public.rol(id);


--
-- TOC entry 4821 (class 2606 OID 28067)
-- Name: vendedor vendedor_fkcodpersona_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.vendedor
    ADD CONSTRAINT vendedor_fkcodpersona_fkey FOREIGN KEY (fkcodpersona) REFERENCES public.persona(codigo);


-- Completed on 2025-09-25 12:27:50

--
-- PostgreSQL database dump complete
--

