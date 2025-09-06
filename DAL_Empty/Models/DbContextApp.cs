using Microsoft.EntityFrameworkCore;

namespace DAL_Empty.Models
{
    public class DbContextApp: DbContext
    {
        public DbContextApp() { }

        public DbContextApp(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           
            optionsBuilder.UseSqlServer("Server=LAP4HUNG;Database=DATN100;Trusted_Connection=True;TrustServerCertificate=true;");

        }
        public virtual DbSet<Account> Accounts { get; set; }

        public virtual DbSet<Address> Addresses { get; set; }

        public virtual DbSet<OrderHistory> OrderHistories { get; set; }

        public virtual DbSet<Brand> Brands { get; set; }

        public virtual DbSet<Cart> Carts { get; set; }

        public virtual DbSet<CartItem> CartItems { get; set; }

        public virtual DbSet<Category> Categories { get; set; }

        //public virtual DbSet<ChatMessage> ChatMessages { get; set; }

        //public virtual DbSet<ChatSession> ChatSessions { get; set; }

        public virtual DbSet<Color> Colors { get; set; }

        public virtual DbSet<Customer> Customers { get; set; }

        public virtual DbSet<CustomerVoucher> CustomerVouchers { get; set; }

        public virtual DbSet<Image> Images { get; set; }

        public virtual DbSet<Material> Materials { get; set; }

        public virtual DbSet<ModeOfPayment> ModeOfPayments { get; set; }

        public virtual DbSet<ModeOfPaymentOrder> ModeOfPaymentOrders { get; set; }

        public virtual DbSet<OrderDetail> OrderDetails { get; set; }

        public virtual DbSet<OrderInfo> OrderInfos { get; set; }

        public virtual DbSet<OrderPaymentMethod> OrderPaymentMethods { get; set; }

        public virtual DbSet<Origin> Origins { get; set; }

        public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

        public virtual DbSet<Product> Products { get; set; }

        public virtual DbSet<ProductDetail> ProductDetails { get; set; }

        public virtual DbSet<Promotion> Promotions { get; set; }

        public virtual DbSet<PromotionProduct> PromotionProducts { get; set; }

        //public virtual DbSet<Rating> Ratings { get; set; }

        //public virtual DbSet<ReturnRequest> ReturnRequests { get; set; }

        public virtual DbSet<Role> Roles { get; set; }

        public virtual DbSet<Size> Sizes { get; set; }

        public virtual DbSet<Supplier> Suppliers { get; set; }

        public virtual DbSet<Voucher> Vouchers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Account__3214EC27DED7DA42");

                entity.ToTable("Account");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("ID");
                entity.Property(e => e.Birthday).HasColumnType("datetime");
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Gender).HasMaxLength(10);
                entity.Property(e => e.Name).HasMaxLength(20);
                entity.Property(e => e.Password).HasMaxLength(255);
                entity.Property(e => e.PhoneNumber).HasMaxLength(15);
                entity.Property(e => e.UserName).HasMaxLength(100);

                entity.HasOne(d => d.Role).WithMany(p => p.Accounts)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK__Account__RoleId__1CBC4616");
            });

            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Address__3214EC2726A43D6A");

                entity.ToTable("Address");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("ID");
                //entity.Property(e => e.City).HasMaxLength(100);
                //entity.Property(e => e.Country).HasMaxLength(100);
                //entity.Property(e => e.State).HasMaxLength(50);
                //entity.Property(e => e.Street).HasMaxLength(100);

                entity.HasOne(d => d.Customer).WithMany(p => p.Addresses)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK__Address__Custome__7E37BEF6");
            });

            modelBuilder.Entity<OrderHistory>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__BillHist__3214EC274EE17F8F");

                entity.ToTable("BillHistory");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("ID");

                entity.HasOne(d => d.Bill).WithMany(p => p.BillHistories)
                    .HasForeignKey(d => d.BillId)
                    .HasConstraintName("FK__BillHisto__BillI__2CF2ADDF");
            });

            modelBuilder.Entity<Brand>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Brand__3214EC276639C5AD");

                entity.ToTable("Brand");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("ID");
                entity.Property(e => e.Code)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Cart__3214EC27A2EB50A7");

                entity.ToTable("Cart");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("ID");
                entity.Property(e => e.CreateAt).HasColumnType("datetime");
               

                entity.HasOne(d => d.Customer).WithOne(p => p.Cart)
                    .HasForeignKey<Cart>(cart => cart.CustomerId)
                    .HasConstraintName("FK__Cart__CustomerId__02084FDA");
            });

            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__CartItem__3214EC27017E44FA");

                entity.ToTable("CartItem");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("ID");
                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

                entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                    .HasForeignKey(d => d.CartId)
                    .HasConstraintName("FK__CartItem__CartId__05D8E0BE");

                entity.HasOne(d => d.ProductDetail).WithMany(p => p.CartItems)
                    .HasForeignKey(d => d.ProductDetailId)
                    .HasConstraintName("FK__CartItem__Produc__06CD04F7");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Category__3214EC27664D474C");

                entity.ToTable("Category");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("ID");
                entity.Property(e => e.Name).HasMaxLength(100);
            });

            //modelBuilder.Entity<ChatMessage>(entity =>
            //{
            //    entity.HasKey(e => e.Id).HasName("PK__ChatMess__3214EC27B941F150");

            //    entity.ToTable("ChatMessage");

            //    entity.Property(e => e.Id)
            //        .HasDefaultValueSql("(newid())")
            //        .HasColumnName("ID");
            //    entity.Property(e => e.SendAt).HasColumnType("datetime");
            //    entity.Property(e => e.Sender).HasMaxLength(50);

            //    entity.HasOne(d => d.ChatSession).WithMany(p => p.ChatMessages)
            //        .HasForeignKey(d => d.ChatSessionId)
            //        .HasConstraintName("FK__ChatMessa__ChatS__0E6E26BF");
            //});

            //modelBuilder.Entity<ChatSession>(entity =>
            //{
            //    entity.HasKey(e => e.Id).HasName("PK__ChatSess__3214EC278B069E58");

            //    entity.ToTable("ChatSession");

            //    entity.Property(e => e.Id)
            //        .HasDefaultValueSql("(newid())")
            //        .HasColumnName("ID");
            //    entity.Property(e => e.CreateAt).HasColumnType("datetime");
            //    entity.Property(e => e.Status).HasMaxLength(20);

            //    entity.HasOne(d => d.Customer).WithMany(p => p.ChatSessions)
            //        .HasForeignKey(d => d.CustomerId)
            //        .HasConstraintName("FK__ChatSessi__Custo__0A9D95DB");
            //});

            modelBuilder.Entity<Color>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Color__3214EC273972E99E");

                entity.ToTable("Color");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("ID");
                entity.Property(e => e.Code)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC27E9A68F92");

                entity.ToTable("Customer");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("ID");
                entity.Property(e => e.Birthday).HasColumnType("datetime");
                entity.Property(e => e.CreateAt).HasColumnType("datetime");
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Fullname).HasMaxLength(100);
                entity.Property(e => e.Gender).HasMaxLength(10);
                entity.Property(e => e.Password).IsUnicode(false) ;
                entity.Property(e => e.PhoneNumber).HasMaxLength(15);
                entity.Property(e => e.UpdateAt).HasColumnType("datetime");
                entity.Property(e => e.UserName).HasMaxLength(100);
                //entity.Property(e => e.Status).HasMaxLength(20);
            });

            modelBuilder.Entity<CustomerVoucher>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC27D74ECC95");

                entity.ToTable("CustomerVoucher");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("ID");
                entity.Property(e => e.UsedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Customer).WithMany(p => p.CustomerVouchers)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK__CustomerV__Custo__151B244E");

                entity.HasOne(d => d.Voucher).WithMany(p => p.CustomerVouchers)
                    .HasForeignKey(d => d.VoucherId)
                    .HasConstraintName("FK__CustomerV__Vouch__160F4887");
            });

            modelBuilder.Entity<Image>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Image__3214EC278359829B");

                entity.ToTable("Image");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("ID");
                entity.Property(e => e.Url)
                    .HasMaxLength(500)
                    .HasColumnName("URL");

                entity.HasOne(d => d.ProductDetail).WithMany(p => p.Images)
                    .HasForeignKey(d => d.ProductDetailId)
                    .HasConstraintName("FK__Image__ProductDe__6FE99F9F");
            });

            modelBuilder.Entity<Material>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Material__3214EC27025D3F05");

                entity.ToTable("Material");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("ID");
                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<ModeOfPayment>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__ModeOfPa__3214EC2777E5FDA7");

                entity.ToTable("ModeOfPayment");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("ID");
                entity.Property(e => e.CreationDate).HasColumnType("datetime");
                entity.Property(e => e.Creator).HasMaxLength(100);
                entity.Property(e => e.EditDate).HasColumnType("datetime");
                entity.Property(e => e.Fixer).HasMaxLength(100);
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(20);
            });

            modelBuilder.Entity<ModeOfPaymentOrder>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__ModeOfPa__3214EC276D701296");

                entity.ToTable("ModeOfPaymentOrder");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("ID");

                entity.HasOne(d => d.ModeOfPayment).WithMany(p => p.ModeOfPaymentOrders)
                    .HasForeignKey(d => d.ModeOfPaymentId)
                    .HasConstraintName("FK__ModeOfPay__ModeO__3D2915A8");

                entity.HasOne(d => d.Order).WithMany(p => p.ModeOfPaymentOrders)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK__ModeOfPay__Order__3C34F16F");
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__OrderDet__3214EC2763A61002");

                entity.ToTable("OrderDetail");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("ID");
                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

                entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK__OrderDeta__Order__30C33EC3");

                entity.HasOne(d => d.ProductDetail).WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.ProductDetailId)
                    .HasConstraintName("FK__OrderDeta__Produ__31B762FC");
            });

            modelBuilder.Entity<OrderInfo>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__OrderInf__3214EC273A145236");

                entity.ToTable("OrderInfo");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("ID");
                entity.Property(e => e.CreateAt).HasColumnType("datetime");
                entity.Property(e => e.CustomerName).HasMaxLength(100);
                entity.Property(e => e.EstimatedDeliveryDate).HasColumnType("datetime");
                entity.Property(e => e.PhoneNumber).HasMaxLength(15);
                entity.Property(e => e.Qrcode)
                    .IsUnicode(false)
                    .HasColumnName("QRCode");
                entity.Property(e => e.ShippingFee).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.UpdateAt).HasColumnType("datetime");

                entity.HasOne(d => d.Customer).WithMany(p => p.OrderInfos)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK__OrderInfo__Custo__2180FB33");

                entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.OrderInfos)
                    .HasForeignKey(d => d.UpdateBy)
                    .HasConstraintName("FK__OrderInfo__Updat__208CD6FA");
                entity.HasOne(d => d.CreateByNavigation).WithMany()
                    .HasForeignKey(d => d.CreateBy)
                    .HasConstraintName("FK__OrderInfo__Creat__XXXXXX");
            });

            modelBuilder.Entity<OrderPaymentMethod>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__OrderPay__3214EC2704BCF665");

                entity.ToTable("OrderPaymentMethod");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("ID");

                entity.HasOne(d => d.Order).WithMany(p => p.OrderPaymentMethods)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK__OrderPaym__Order__282DF8C2");

                entity.HasOne(d => d.PaymentMethod).WithMany(p => p.OrderPaymentMethods)
                    .HasForeignKey(d => d.PaymentMethodId)
                    .HasConstraintName("FK__OrderPaym__Payme__29221CFB");
            });

            modelBuilder.Entity<Origin>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Origin__3214EC27501CB207");

                entity.ToTable("Origin");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("ID");
                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<PaymentMethod>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__PaymentM__3214EC27CE8078EB");

                entity.ToTable("PaymentMethod");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("ID");
                entity.Property(e => e.Name).HasMaxLength(100);
               
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Product__3214EC27D54E6411");

                entity.ToTable("Product");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("ID");
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");
                entity.Property(e => e.Name).HasMaxLength(500);
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
                entity.HasOne(d => d.Brand).WithMany(p => p.Product)
                   .HasForeignKey(d => d.BrandId)
                   .HasConstraintName("FK__ProductDe__Brand__66603565");

                entity.HasOne(d => d.Category).WithMany(p => p.Products)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("FK__ProductDe__Categ__656C112C");
            });

            modelBuilder.Entity<ProductDetail>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__ProductD__3214EC2711BA148D");

                entity.ToTable("ProductDetail");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("ID");
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

               

                entity.HasOne(d => d.Color).WithMany(p => p.ProductDetails)
                    .HasForeignKey(d => d.ColorId)
                    .HasConstraintName("FK__ProductDe__Color__628FA481");

                entity.HasOne(d => d.Material).WithMany(p => p.ProductDetails)
                    .HasForeignKey(d => d.MaterialId)
                    .HasConstraintName("FK__ProductDe__Mater__6477ECF3");

                entity.HasOne(d => d.Origin).WithMany(p => p.ProductDetails)
                    .HasForeignKey(d => d.OriginId)
                    .HasConstraintName("FK__ProductDe__Origi__6754599E");

                entity.HasOne(d => d.Product).WithMany(p => p.ProductDetails)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK__ProductDe__Produ__619B8048");

                entity.HasOne(d => d.Size).WithMany(p => p.ProductDetails)
                    .HasForeignKey(d => d.SizeId)
                    .HasConstraintName("FK__ProductDe__SizeI__6383C8BA");

                entity.HasOne(d => d.Supplier).WithMany(p => p.ProductDetails)
                    .HasForeignKey(d => d.SupplierId)
                    .HasConstraintName("FK__ProductDe__Suppl__68487DD7");
            });

            modelBuilder.Entity<Promotion>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Promotio__3214EC27175E4A9C");

                entity.ToTable("Promotion");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("ID");
                entity.Property(e => e.DiscountType).HasMaxLength(100);
                entity.Property(e => e.DiscountValue).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.EndDate).HasColumnType("datetime");
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.StartDate).HasColumnType("datetime");
                entity.Property(e => e.Status).HasMaxLength(50);
            });

            modelBuilder.Entity<PromotionProduct>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Promotio__3214EC275E306DB8");

                entity.ToTable("PromotionProduct");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("ID");

                entity.HasOne(d => d.ProductDetail).WithMany(p => p.PromotionProducts)
                    .HasForeignKey(d => d.ProductDetailId)
                    .HasConstraintName("FK__Promotion__Produ__778AC167");

                entity.HasOne(d => d.Promotion).WithMany(p => p.PromotionProducts)
                    .HasForeignKey(d => d.PromotionId)
                    .HasConstraintName("FK__Promotion__Promo__76969D2E");
                entity.Property(e => e.Priceafterduction).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.Pricebeforereduction).HasColumnType("decimal(18, 2)");
            });

            //modelBuilder.Entity<Rating>(entity =>
            //{
            //    entity.HasKey(e => e.Id).HasName("PK__Rating__3214EC27BBD31BB6");

            //    entity.ToTable("Rating");

            //    entity.Property(e => e.Id)
            //        .HasDefaultValueSql("(newid())")
            //        .HasColumnName("ID");
            //    entity.Property(e => e.CreateAt).HasColumnType("datetime");
            //    entity.Property(e => e.Status).HasMaxLength(100);
            //    entity.Property(e => e.UpdateAt).HasColumnType("datetime");

            //    entity.HasOne(d => d.ProductDetail).WithMany(p => p.Ratings)
            //        .HasForeignKey(d => d.ProductDetailId)
            //        .HasConstraintName("FK__Rating__ProductD__6C190EBB");
            //});

            //modelBuilder.Entity<ReturnRequest>(entity =>
            //{
            //    entity.HasKey(e => e.Id).HasName("PK__ReturnRe__3214EC271C31DF75");

            //    entity.ToTable("ReturnRequest");

            //    entity.Property(e => e.Id)
            //        .HasDefaultValueSql("(newid())")
            //        .HasColumnName("ID");
            //    entity.Property(e => e.RequestDate).HasColumnType("datetime");
            //    entity.Property(e => e.Status).HasMaxLength(20);

            //    entity.HasOne(d => d.OrderDetail).WithMany(p => p.ReturnRequests)
            //        .HasForeignKey(d => d.OrderDetailId)
            //        .HasConstraintName("FK__ReturnReq__Order__3587F3E0");
            //});

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Role__3214EC2748685E40");

                entity.ToTable("Role");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("ID");
                entity.Property(e => e.Name).HasMaxLength(20);
            });

            modelBuilder.Entity<Size>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Size__3214EC27F999BE80");

                entity.ToTable("Size");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("ID");
                entity.Property(e => e.Code)
                    .HasMaxLength(20)
                    .IsUnicode(false);
                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Supplier__3214EC27DF0A55B7");

                entity.ToTable("Supplier");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("ID");
                entity.Property(e => e.Contact).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Voucher>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Voucher__3214EC27BFD519B4");

                entity.ToTable("Voucher");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())")
                    .HasColumnName("ID");
                entity.Property(e => e.Code).HasMaxLength(50);
                entity.Property(e => e.DiscountValue).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.EndDate).HasColumnType("datetime");
                entity.Property(e => e.StartDate).HasColumnType("datetime");
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.Description).HasColumnType("nvarchar(255)").IsUnicode(true).IsRequired(false);
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}
