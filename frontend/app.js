const API_URL = "https://app-techstore-api-natanael-h7dvb7egdfcucfgv.brazilsouth-01.azurewebsites.net/api/produtos";
const TAMANHO_PAGINA = 10;

const elemento = (id) => document.getElementById(id);

let paginaAtual = 1;
let filtroNome = "";
let produtoEmEdicao = null;
let produtosDaPagina = [];
let carregando = false;

const moeda = new Intl.NumberFormat("pt-BR", {
    style: "currency",
    currency: "BRL"
});

function mostrarMensagem(texto, erro = false) {
    elemento("mensagem").textContent = texto;
    elemento("mensagem").classList.toggle("erro", erro);
}

async function requisitar(url, options = {}) {
    const resposta = await fetch(url, options);

    if (resposta.status === 204) {
        return null;
    }

    const dados = await resposta.json().catch(() => null);

    if (!resposta.ok) {
        const errosValidacao = dados?.errors
            ? Object.values(dados.errors).flat().join(" ")
            : null;

        throw new Error(
            errosValidacao ||
            dados?.detail ||
            dados?.title ||
            `Falha na operação: HTTP ${resposta.status}.`
        );
    }

    return dados;
}

function atualizarPaginacao() {
    elemento("pagina").textContent = `Página ${paginaAtual}`;

    elemento("anterior").disabled =
        carregando || paginaAtual === 1;

    elemento("proxima").disabled =
        carregando || produtosDaPagina.length < TAMANHO_PAGINA;
}

function adicionarCelula(linha, valor) {
    const celula = document.createElement("td");
    celula.textContent = valor;
    linha.appendChild(celula);
}

function renderizarProdutos() {
    const tabela = elemento("produtos");
    tabela.replaceChildren();

    if (produtosDaPagina.length === 0) {
        const linha = document.createElement("tr");
        const celula = document.createElement("td");

        celula.colSpan = 6;
        celula.textContent = "Nenhum produto encontrado.";

        linha.appendChild(celula);
        tabela.appendChild(linha);
        return;
    }

    for (const produto of produtosDaPagina) {
        const linha = document.createElement("tr");

        adicionarCelula(linha, produto.codigo);
        adicionarCelula(linha, produto.nome);
        adicionarCelula(linha, moeda.format(produto.preco));
        adicionarCelula(linha, produto.categoria);
        adicionarCelula(linha, produto.ativo ? "Ativo" : "Inativo");

        const acoes = document.createElement("td");

        const editar = document.createElement("button");
        editar.type = "button";
        editar.className = "secundario";
        editar.textContent = "Editar";
        editar.addEventListener("click", () => editarProduto(produto));

        const excluir = document.createElement("button");
        excluir.type = "button";
        excluir.className = "perigo";
        excluir.textContent = "Excluir";
        excluir.addEventListener("click", () =>
            excluirProduto(produto, excluir));

        acoes.append(editar, excluir);
        linha.appendChild(acoes);
        tabela.appendChild(linha);
    }
}

async function carregarProdutos() {
    carregando = true;
    atualizarPaginacao();

    try {
        const parametros = new URLSearchParams({
            pagina: paginaAtual,
            tamanhoPagina: TAMANHO_PAGINA
        });

        if (filtroNome) {
            parametros.set("nome", filtroNome);
        }

        let produtos = await requisitar(`${API_URL}?${parametros}`);

        // Volta uma página caso a página atual fique vazia.
        if (produtos.length === 0 && paginaAtual > 1) {
            paginaAtual--;
            parametros.set("pagina", paginaAtual);

            produtos = await requisitar(`${API_URL}?${parametros}`);
        }

        produtosDaPagina = produtos;
        renderizarProdutos();
    } finally {
        carregando = false;
        atualizarPaginacao();
    }
}

function limparFormulario() {
    produtoEmEdicao = null;
    elemento("form-produto").reset();
    elemento("titulo-formulario").textContent = "Cadastrar produto";
    elemento("salvar").textContent = "Salvar produto";
}

function editarProduto(produto) {
    produtoEmEdicao = produto.id;

    elemento("codigo").value = produto.codigo;
    elemento("nome").value = produto.nome;
    elemento("descricao").value = produto.descricao ?? "";
    elemento("preco").value = produto.preco;
    elemento("categoria").value = produto.categoria;
    elemento("ativo").checked = produto.ativo;

    elemento("titulo-formulario").textContent =
        `Editar produto #${produto.id}`;

    elemento("salvar").textContent = "Salvar alterações";
    elemento("codigo").focus();
}

elemento("form-produto").addEventListener("submit", async (evento) => {
    evento.preventDefault();

    const botao = elemento("salvar");
    botao.disabled = true;
    elemento("cancelar").disabled = true;

    const editando = produtoEmEdicao !== null;

    const produto = {
        codigo: elemento("codigo").value.trim(),
        nome: elemento("nome").value.trim(),
        descricao: elemento("descricao").value.trim() || null,
        preco: Number(elemento("preco").value),
        categoria: elemento("categoria").value.trim(),
        ativo: elemento("ativo").checked
    };

    try {
        const url = editando
            ? `${API_URL}/${produtoEmEdicao}`
            : API_URL;

        await requisitar(url, {
            method: editando ? "PUT" : "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(produto)
        });
    } catch (erro) {
        mostrarMensagem(erro.message, true);
        botao.disabled = false;
        elemento("cancelar").disabled = false;
        return;
    }

    limparFormulario();
    botao.disabled = false;
    elemento("cancelar").disabled = false;

    mostrarMensagem(
        editando
            ? "Produto atualizado com sucesso."
            : "Produto cadastrado com sucesso."
    );

    try {
        await carregarProdutos();
    } catch (erro) {
        mostrarMensagem(
            `Produto salvo, mas não foi possível atualizar a lista. ${erro.message}`,
            true
        );
    }
});

async function excluirProduto(produto, botao) {
    if (!confirm(`Excluir o produto "${produto.nome}"?`)) {
        return;
    }

    botao.disabled = true;

    try {
        await requisitar(`${API_URL}/${produto.id}`, {
            method: "DELETE"
        });
    } catch (erro) {
        mostrarMensagem(erro.message, true);
        botao.disabled = false;
        return;
    }

    if (produtoEmEdicao === produto.id) {
        limparFormulario();
    }

    mostrarMensagem("Produto excluído com sucesso.");

    try {
        await carregarProdutos();
    } catch (erro) {
        mostrarMensagem(
            `Produto excluído, mas não foi possível atualizar a lista. ${erro.message}`,
            true
        );
    }
}

elemento("cancelar").addEventListener("click", limparFormulario);

elemento("form-busca").addEventListener("submit", async (evento) => {
    evento.preventDefault();

    if (carregando) return;

    filtroNome = elemento("busca").value.trim();
    paginaAtual = 1;

    try {
        await carregarProdutos();
    } catch (erro) {
        mostrarMensagem(erro.message, true);
    }
});

elemento("anterior").addEventListener("click", async () => {
    if (carregando || paginaAtual === 1) return;

    paginaAtual--;

    try {
        await carregarProdutos();
    } catch (erro) {
        paginaAtual++;
        atualizarPaginacao();
        mostrarMensagem(erro.message, true);
    }
});

elemento("proxima").addEventListener("click", async () => {
    if (carregando) return;

    paginaAtual++;

    try {
        await carregarProdutos();
    } catch (erro) {
        paginaAtual--;
        atualizarPaginacao();
        mostrarMensagem(erro.message, true);
    }
});

carregarProdutos().catch((erro) =>
    mostrarMensagem(
        `Não foi possível carregar os produtos. Confira se a API está executando. ${erro.message}`,
        true
    )
);