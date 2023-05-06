

select * from [dbo].[Compra]

select * from [dbo].[DetallesCompra]



alter procedure sp_compraProducto
@StringXML varchar(MAX)
AS
BEGIN
	BEGIN TRANSACTION 
		SET NOCOUNT ON;
		Declare @XML XML
		declare @Compra integer;

		---------------------
		BEGIN TRY
		set @XML = @StringXML	
		---insertar datos compra 
		
		INSERT INTO [dbo].[Compra]
		(iva_compra,subtotal,TotalCompra)
		select 
		R.Item.query('./iva_compra').value('.','decimal(10, 2)') 
		,R.Item.query('./subtotal').value('.','decimal(10, 2)') 
		,R.Item.query('./TotalCompra').value('.','decimal(10, 2)') 
		from @XML.nodes('./Compra/item') as R(Item)
		-----extraigo id
		SET @Compra = SCOPE_IDENTITY();

		-------------compra -producto
		insert into  [dbo].[DetallesCompra]  
		(IDCompra,nombre_product,Cantidad,PrecioUnitario,ivaproducto,Subtotal,total,descripcion,fecha_vencimiento)							
		select @Compra,
		R.Item.query('./nombre_product').value('.','varchar(30)')
		,R.Item.query('./Cantidad').value('.','int')
		,R.Item.query('./PrecioUnitario').value('.','decimal(10, 2)')
		,R.Item.query('./ivaproducto').value('.','decimal(10, 2)')
		,R.Item.query('./Subtotal').value('.','decimal(10, 2)')
		,R.Item.query('./total').value('.','decimal(10, 2)')
		,R.Item.query('./descripcion').value('.','varchar(30)')
		,R.Item.query('./fecha_vencimiento').value('.','date')
		from @XML.nodes('./Detalle_Compra/item') as R(Item)

			COMMIT TRANSACTION 
		END TRY
		BEGIN CATCH
			ROLLBACK TRAN
			
			END CATCH;
		
END