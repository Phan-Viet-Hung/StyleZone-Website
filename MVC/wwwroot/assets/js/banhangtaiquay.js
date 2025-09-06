document.addEventListener('DOMContentLoaded', function() {
        let cart = [];
        const currentTime = new Date().toLocaleString('en-US', { timeZone: 'Asia/Ho_Chi_Minh', hour12: true });
        const productSearch = document.getElementById('productSearch');
        const productName = document.getElementById('productName');
        const productCode = document.getElementById('productCode');
        const productPrice = document.getElementById('productPrice');
        const colorSelect = document.getElementById('colorSelect');
        const sizeSelect = document.getElementById('sizeSelect');
        const quantity = document.getElementById('quantity');
        const discount = document.getElementById('discount');
        const addToCartBtn = document.getElementById('addToCartBtn');
        const posCartBody = document.getElementById('posCartBody');
        const subtotalElement = document.getElementById('subtotal');
        const discountAmountElement = document.getElementById('discountAmount');
        const totalElement = document.getElementById('total');
        const completeSaleBtn = document.getElementById('completeSale');
        const productTableBody = document.getElementById('productTableBody');

        // Full static product data
        const products = {
          '1': { name: 'Laptop Model X', code: 'LPT-X001', price: 999.99, colors: ['Black', 'Silver', 'Blue'], sizes: ['13"', '15"', '17"'], image: 'https://via.placeholder.com/50?text=Laptop' },
          '2': { name: 'Smartphone Model Y', code: 'SPH-Y002', price: 599.99, colors: ['Black', 'White', 'Gold'], sizes: ['6.1"', '6.7"'], image: 'https://via.placeholder.com/50?text=Smartphone' },
          '3': { name: 'Headphones Model Z', code: 'HDP-Z003', price: 149.99, colors: ['Black', 'Red', 'White'], sizes: ['Small', 'Large'], image: 'https://via.placeholder.com/50?text=Headphones' },
          '4': { name: 'Tablet Model A', code: 'TBL-A004', price: 399.99, colors: ['Gray', 'Silver'], sizes: ['10"', '12"'], image: 'https://via.placeholder.com/50?text=Tablet' },
          '5': { name: 'Smartwatch Model B', code: 'SWT-B005', price: 199.99, colors: ['Black', 'Silver'], sizes: ['Small', 'Medium', 'Large'], image: 'https://via.placeholder.com/50?text=Smartwatch' }
        };

        // Function to update product table based on search
        function updateProductTable(searchTerm) {
          productTableBody.innerHTML = '';
          const filteredProducts = Object.values(products).filter(product =>
            product.name.toLowerCase().includes(searchTerm.toLowerCase())
          );
          filteredProducts.forEach(product => {
            const row = document.createElement('tr');
            row.innerHTML = `
              <td><img src="${product.image}" alt="${product.name}" class="cart-image"></td>
              <td>${product.name}</td>
              <td>${product.code}</td>
              <td>$${product.price.toFixed(2)}</td>
              <td><button class="main-btn primary-btn btn-hover action-btn" onclick="addProduct('${product.name}')">Add</button></td>
            `;
            productTableBody.appendChild(row);
          });
        }

        // Autocomplete and search integration
        productSearch.addEventListener('input', function() {
          const val = this.value;
          updateProductTable(val);
        });

        // Initial load of product table
        updateProductTable('');

        // Add product to form
        window.addProduct = function(productName) {
          const product = Object.values(products).find(p => p.name === productName);
          if (product) {
            productSearch.value = product.name;
            updateDetails(product);
          }
        };

        function updateDetails(product) {
          colorSelect.innerHTML = '<option value="">Select Color</option>';
          sizeSelect.innerHTML = '<option value="">Select Size</option>';
          if (product) {
            productName.value = product.name;
            productCode.value = product.code;
            productPrice.value = `$${product.price.toFixed(2)}`;
            product.colors.forEach(color => {
              const option = document.createElement('option');
              option.value = color;
              option.text = color;
              colorSelect.appendChild(option);
            });
            product.sizes.forEach(size => {
              const option = document.createElement('option');
              option.value = size;
              option.text = size;
              sizeSelect.appendChild(option);
            });
          } else {
            productName.value = '';
            productCode.value = '';
            productPrice.value = '';
          }
        }

        addToCartBtn.addEventListener('click', function() {
          const quantityValue = parseInt(quantity.value);
          const discountValue = parseFloat(discount.value) || 0;
          const color = colorSelect.value;
          const size = sizeSelect.value;

          if (productName.value && color && size && quantityValue > 0) {
            const product = Object.values(products).find(p => p.name === productName.value);
            if (product) {
              const item = {
                name: product.name,
                code: product.code,
                image: product.image,
                color,
                size,
                price: product.price,
                quantity: quantityValue,
                discount: discountValue
              };
              cart.push(item);
              updateCart();
              productSearch.value = '';
              updateProductTable('');
              updateDetails(null);
              productSearch.focus();
            }
          }
        });

        function updateCart() {
          posCartBody.innerHTML = '';
          let subtotal = 0;
          cart.forEach((item, index) => {
            const itemTotal = item.price * item.quantity;
            const discountAmount = itemTotal * (item.discount / 100);
            const finalTotal = itemTotal - discountAmount;
            subtotal += finalTotal;

            const row = document.createElement('tr');
            row.innerHTML = `
              <td><img src="${item.image}" alt="${item.name}" class="cart-image"></td>
              <td>${item.name}</td>
              <td>${item.code}</td>
              <td>${item.color}</td>
              <td>${item.size}</td>
              <td>${item.price.toFixed(2)}</td>
              <td>${item.quantity}</td>
              <td>${finalTotal.toFixed(2)}</td>
              <td><button class="main-btn danger-btn btn-hover" onclick="removeItem(${index})" style="padding: 8px;">Remove</button></td>
            `;
            posCartBody.appendChild(row);
          });

          const totalDiscount = subtotal * (discount.value / 100) || 0;
          const grandTotal = subtotal - totalDiscount;

          subtotalElement.textContent = subtotal.toFixed(2);
          discountAmountElement.textContent = totalDiscount.toFixed(2);
          totalElement.textContent = grandTotal.toFixed(2);
        }

        window.removeItem = function(index) {
          cart.splice(index, 1);
          updateCart();
        };

        completeSaleBtn.addEventListener('click', function() {
          if (cart.length > 0) {
            const receipt = `
              **Sale Receipt**
              Date: ${new Date().toLocaleDateString('en-US', { timeZone: 'Asia/Ho_Chi_Minh' })}
              Time: ${currentTime}
              --------------------------------
              ${cart.map(item => `${item.name} (Code: ${item.code}, ${item.color}, ${item.size}) - $${(item.price * item.quantity).toFixed(2)} (${item.quantity} x $${item.price.toFixed(2)}, Discount: ${item.discount}%)`).join('\n')}
              --------------------------------
              Subtotal: $${subtotalElement.textContent}
              Discount: $${discountAmountElement.textContent}
              Total: $${totalElement.textContent}
              --------------------------------
              Thank you for your purchase!
            `;
            alert(receipt); // For demo, use alert; in production, use print
            cart = [];
            updateCart();
          } else {
            alert('No items in cart to complete sale!');
          }
        });
      });