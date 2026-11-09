(() => {
    const cart = new Map();
    const formatter = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });
    const cartItems = document.getElementById("cart-items");
    const cartCount = document.getElementById("cart-count");
    const cartTotal = document.getElementById("cart-total");
    const stickyCart = document.getElementById("cart-sticky");
    const stickyCartCount = document.getElementById("sticky-cart-count");
    const stickyCartTotal = document.getElementById("sticky-cart-total");
    const noSearchResults = document.getElementById("no-search-results");
    const emptyMessage = document.getElementById("cart-empty");
    const orderForm = document.getElementById("checkout-form");
    const deliveryType = document.getElementById("TipoAtendimento");
    const addressField = document.getElementById("address-field");
    const addressInput = document.getElementById("EnderecoEntrega");

    function renderCart() {
        cartItems.replaceChildren();
        let total = 0;
        let count = 0;

        for (const [id, item] of cart) {
            const subtotal = item.price * item.quantity;
            total += subtotal;
            count += item.quantity;

            const row = document.createElement("div");
            row.className = "d-flex justify-content-between align-items-center gap-3 py-2 border-bottom";

            const details = document.createElement("div");
            const name = document.createElement("strong");
            name.textContent = item.name;
            const price = document.createElement("div");
            price.className = "small text-muted";
            price.textContent = `${item.quantity} × ${formatter.format(item.price)}`;
            details.append(name, price);

            const controls = document.createElement("div");
            controls.className = "d-flex align-items-center gap-2";
            const minus = document.createElement("button");
            minus.type = "button";
            minus.className = "btn btn-sm btn-outline-secondary";
            minus.textContent = "−";
            minus.setAttribute("aria-label", `Remover um ${item.name}`);
            minus.addEventListener("click", () => changeQuantity(id, -1));
            const quantity = document.createElement("span");
            quantity.textContent = String(item.quantity);
            const plus = document.createElement("button");
            plus.type = "button";
            plus.className = "btn btn-sm btn-outline-secondary";
            plus.textContent = "+";
            plus.setAttribute("aria-label", `Adicionar um ${item.name}`);
            plus.disabled = item.quantity >= 50;
            plus.addEventListener("click", () => changeQuantity(id, 1));
            controls.append(minus, quantity, plus);
            row.append(details, controls);
            cartItems.append(row);
        }

        cartCount.textContent = String(count);
        cartTotal.textContent = formatter.format(total);
        stickyCartCount.textContent = String(count);
        stickyCartTotal.textContent = formatter.format(total);
        stickyCart.classList.toggle("is-visible", count > 0);
        emptyMessage.classList.toggle("d-none", count > 0);
    }

    function changeQuantity(id, difference) {
        const item = cart.get(id);
        if (!item) return;
        if (difference > 0 && item.quantity >= 50) return;
        item.quantity += difference;
        if (item.quantity <= 0) cart.delete(id);
        renderCart();
    }

    document.querySelectorAll(".add-to-cart").forEach(button => {
        button.addEventListener("click", () => {
            const id = button.dataset.id;
            const current = cart.get(id);
            if (current) {
                current.quantity = Math.min(50, current.quantity + 1);
            } else {
                cart.set(id, {
                    name: button.dataset.name,
                    price: Number(button.dataset.price),
                    quantity: 1
                });
            }
            renderCart();
            button.textContent = "Adicionado!";
            window.setTimeout(() => { button.textContent = "Adicionar"; }, 900);
        });
    });

    function filterProducts() {
        const selected = document.querySelector(".category-chip.active")?.dataset.category.toLocaleLowerCase("pt-BR") ?? "all";
        const search = document.getElementById("product-search")?.value.trim().toLocaleLowerCase("pt-BR") ?? "";
        document.querySelectorAll(".product-item").forEach(product => {
            const matchesCategory = selected === "all" || product.dataset.category.toLocaleLowerCase("pt-BR") === selected;
            const matchesSearch = product.dataset.name.toLocaleLowerCase("pt-BR").includes(search);
            product.classList.toggle("d-none", !matchesCategory || !matchesSearch);
        });
        const visibleProducts = Array.from(document.querySelectorAll(".product-item"))
            .some(product => !product.classList.contains("d-none"));
        noSearchResults?.classList.toggle("d-none", visibleProducts);
    }

    document.getElementById("category-filters")?.addEventListener("click", event => {
        const button = event.target.closest("[data-category]");
        if (!button) return;
        document.querySelectorAll(".category-chip").forEach(chip => chip.classList.toggle("active", chip === button));
        filterProducts();
    });

    document.getElementById("product-search")?.addEventListener("input", filterProducts);
    document.querySelectorAll(".product-photo").forEach(image => {
        image.addEventListener("error", () => {
            image.src = image.dataset.fallback;
            image.classList.remove("product-photo");
            image.classList.add("product-placeholder");
            image.alt = "";
        }, { once: true });
    });

    deliveryType?.addEventListener("change", () => {
        const isDelivery = deliveryType.value === "Entrega";
        addressField.classList.toggle("d-none", !isDelivery);
        addressInput.required = isDelivery;
    });

    orderForm?.addEventListener("submit", event => {
        if (cart.size === 0) {
            event.preventDefault();
            emptyMessage.classList.remove("d-none");
            return;
        }
        document.getElementById("itens-json").value = JSON.stringify(
            Array.from(cart, ([id, item]) => ({ idProduto: Number(id), quantidade: item.quantity }))
        );
    });

    try {
        const previousItems = JSON.parse(document.getElementById("itens-json")?.value || "[]");
        previousItems.forEach(previous => {
            const button = document.querySelector(`.add-to-cart[data-id="${Number(previous.idProduto)}"]`);
            if (!button || !Number.isInteger(previous.quantidade) || previous.quantidade < 1 || previous.quantidade > 50) return;
            cart.set(button.dataset.id, {
                name: button.dataset.name,
                price: Number(button.dataset.price),
                quantity: previous.quantidade
            });
        });
    } catch {
        emptyMessage.textContent = "Não foi possível restaurar o carrinho. Confira os itens e tente novamente.";
        emptyMessage.classList.remove("d-none");
    }

    if (deliveryType?.value === "Entrega") {
        addressField.classList.remove("d-none");
        addressInput.required = true;
    }

    renderCart();
})();
