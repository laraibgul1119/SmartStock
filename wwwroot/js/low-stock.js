document.addEventListener("DOMContentLoaded", () => {
    const container = document.querySelector("[data-low-stock-list]");

    if (!container) {
        return;
    }

    const endpoint = container.dataset.lowStockEndpoint;

    if (!endpoint) {
        return;
    }

    container.innerHTML = "<p class=\"text-muted\">Loading stock information...</p>";

    fetch(endpoint, {
        method: "GET",
        headers: {
            "Accept": "application/json"
        },
        credentials: "same-origin"
    })
        .then(response => {
            if (!response.ok) {
                throw new Error("Unable to load stock information.");
            }

            return response.json();
        })
        .then(products => {
            if (products.length === 0) {
                container.innerHTML =
                    "<p class=\"text-success\">No low-stock products found.</p>";

                return;
            }

            const list = document.createElement("ul");
            list.className = "list-group";

            products.forEach(product => {
                const item = document.createElement("li");
                item.className =
                    "list-group-item d-flex justify-content-between align-items-center";

                const status = product.isOutOfStock
                    ? "Out of stock"
                    : `${product.stockQuantity} remaining`;

                item.innerHTML = `
                    <span>
                        <strong>${escapeHtml(product.name)}</strong>
                        <small class="d-block text-muted">
                            SKU: ${escapeHtml(product.sku)}
                        </small>
                    </span>
                    <span class="badge ${product.isOutOfStock ? "bg-danger" : "bg-warning text-dark"}">
                        ${status}
                    </span>`;

                list.appendChild(item);
            });

            container.replaceChildren(list);
        })
        .catch(() => {
            container.innerHTML =
                "<p class=\"text-danger\">Stock information could not be loaded.</p>";
        });
});

function escapeHtml(value) {
    const element = document.createElement("div");
    element.textContent = value ?? "";
    return element.innerHTML;
}