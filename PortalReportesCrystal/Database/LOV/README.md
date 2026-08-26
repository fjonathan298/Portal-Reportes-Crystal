# Biblioteca de LOV Commands

Archivos SQL estandarizados para listas de valores dinamicas en Crystal Reports.

## Contrato de retorno

Todo LOV Command retorna exactamente 2 columnas:

```
CODIGO       VARCHAR(50)   -- valor que se envia al parametro Crystal
DESCRIPCION  VARCHAR(200)  -- texto visible en el selector
```

## Reglas

1. `CODIGO` siempre como `VARCHAR` (usar `CAST` si el campo es numerico).
2. LOV multi-seleccion incluyen `'TODOS'` como primera fila.
3. LOV en cascada reciben el parametro padre en el WHERE.
4. Nombre del archivo: `LOV_<Entidad>.sql` o `LOV_<Entidad>_Por_<Padre>.sql`.
5. Cada archivo incluye header con proposito, dependencias y ejemplo de uso.

## Guia de contribucion

1. Copiar un LOV existente como plantilla.
2. Respetar el contrato de 2 columnas.
3. Incluir el header estandar.
4. Probar aisladamente en el DW antes de publicar.
5. Agregar entrada en `docs/PATRON_PARAMETROS_DINAMICOS.md` seccion 10.
