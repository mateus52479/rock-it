# 🪨 ROCK IT — Documentação do Projeto

> Jogo clicker 2D desenvolvido em Unity 6, inspirado em *Scritchy Scratchy* e *Cookie Clicker*.  
> Um protagonista sem um real no bolso encontra uma mina e começa a quebrar pedras em busca de pedras preciosas — com a meta de acumular **R$ 1 bilhão**.

---

## 🎮 Visão Geral

O jogador entra em uma mina onde um minerador vende pedras embaladas. Dentro delas pode haver minérios valiosos — ou não. O risco faz parte do jogo. Comprando, quebrando e gerenciando pedras, o jogador evolui seu personagem, desbloqueia habilidades e instala máquinas que automatizam e potencializam a produção.

---

## 🗂️ Estrutura de Cenas

| Cena | Descrição |
|------|-----------|
| `MenuPrincipal` | Tela inicial com opções de novo jogo, continuar, configurações, créditos e sair |
| `Jogo` | Cena principal onde toda a gameplay acontece |

---

## ⚙️ Scripts — Visão Geral

### Gerenciamento Central

| Script | Função |
|--------|--------|
| `GameManager.cs` | Controla o dinheiro do jogador, HUD e animação do contador |
| `SaveManager.cs` | Salva e carrega o progresso (autosave a cada 60s + manual) |
| `GameLoader.cs` | Carrega o save ao entrar na cena Jogo via "Continuar" |
| `PauseManager.cs` | Controla o pause via ESC e o painel de pause |

### Pedras

| Script | Função |
|--------|--------|
| `PedraDados.cs` | Estrutura de dados de uma pedra (nome, cor, preço, recompensa, vida) |
| `PedraDatabase.cs` | Array com todas as pedras disponíveis no jogo |
| `PedraManager.cs` | Controla o comportamento de cada pedra: clique, dano, arrasto, tremor, animações, destruição e recompensa |

### Loja

| Script | Função |
|--------|--------|
| `LojaManager.cs` | Abre/fecha a loja e gerencia a compra e spawn de pedras |
| `ItemLoja.cs` | Configura cada linha da lista de pedras com nome, descrição, preço (verde/vermelho) e botão |
| `NavegacaoLoja.cs` | Controla as abas da loja: Pedras, Status, Habilidades, Máquinas |

### Status

| Script | Função |
|--------|--------|
| `StatusManager.cs` | Gerencia Força, Sorte e Fortuna — níveis, custos e multiplicadores |
| `ItemStatusUI.cs` | Exibe cada status com nível, efeito, custo (verde/vermelho) e botão de upgrade |
| `StatusUIManager.cs` | Instancia os itens de status no ScrollView |

### Habilidades

| Script | Função |
|--------|--------|
| `HabilidadeDados.cs` | Estrutura de dados de uma habilidade |
| `HabilidadeDatabase.cs` | Array com todas as habilidades e seus requisitos |
| `ArvoreHabilidadesUI.cs` | Renderiza a árvore de habilidades em teia com linhas, arrastar e zoom |
| `NoHabilidadeUI.cs` | Controla cada nó da árvore: visual, tooltip, compra e requisitos |

### Máquinas

| Script | Função |
|--------|--------|
| `MaquinaDados.cs` | Estrutura de dados de uma máquina |
| `MaquinaDatabase.cs` | Array com todas as máquinas disponíveis |
| `MaquinaManager.cs` | Gerencia quais máquinas foram compradas e as ativa na cena |
| `MaquinaLixo.cs` | Recebe pedras arrastadas e as descarta (com ou sem cashback no Lixo de Luxo) |
| `MaquinaScanner.cs` | Adiciona contorno verde/vermelho em todas as pedras da cena |
| `MaquinaQuebrador.cs` | Quebra pedras automaticamente a X dano por segundo (evoluível) |
| `MaquinaInversor.cs` | Inverte o sinal do valor de recompensa de uma pedra |
| `ItemMaquinaUI.cs` | Exibe cada máquina na loja com botão Comprar/Evoluir/Comprado |
| `MaquinaUIManager.cs` | Instancia os itens de máquinas no ScrollView |

### Menus e Configurações

| Script | Função |
|--------|--------|
| `MenuPrincipal.cs` | Botões do menu inicial |
| `ConfiguracoesManager.cs` | Volume geral/música/efeitos, resolução, fullscreen e idioma |

### Utilitários

| Script | Função |
|--------|--------|
| `TextoFlutuante.cs` | Texto animado que aparece ao dar dano nas pedras (normal ou crítico) |
| `ComputadorClique.cs` | Abre a loja ao clicar no sprite do computador |

---

## 🪨 Pedras Disponíveis

| Pedra | Preço | Recompensa | Vida |
|-------|-------|------------|------|
| Pedra Comum | Grátis | -2 a +5 | 10 |
| Pedra Arenosa | R$ 10 | -1 a +8 | 6 |
| Pedra Escura | R$ 30 | -5 a +20 | 15 |
| Pedra Cristalina | R$ 60 | 0 a +40 | 12 |
| Pedra Vulcânica | R$ 120 | -10 a +80 | 20 |

---

## 📊 Status do Personagem

| Status | Efeito | Custo base |
|--------|--------|------------|
| Força | Mais dano por clique | R$ 50 |
| Sorte | Tira 2 pontos percentuais dos piores resultados e joga no prêmio máximo | R$ 50 |
| Fortuna | Multiplicador de dinheiro | R$ 100 |

O custo aumenta em **1.4x** a cada nível.

---

## 🌿 Árvore de Habilidades

```
                    Primeiros Passos
                         │
        ┌────────────────┼──────────────────┬──────────────┐
        │                │                  │              │
Desconto         Whey Protein      Trevo de 4 Folhas   Analista     Engenheiro
Universitário                            │            Profissional
        │                          Buquê de Trevos
      Cashback
```

| Habilidade | Efeito | Custo |
|------------|--------|-------|
| Primeiros Passos | Desbloqueia tudo | R$ 100 |
| Desconto Universitário | -25% no preço de pedras | R$ 300 |
| Cashback | 50% de chance de pedra grátis | R$ 600 |
| Whey Protein | 30% de chance de dano crítico (2x) | R$ 400 |
| Trevo de Quatro Folhas | Recompensa máxima vira 2x | R$ 400 |
| Buquê de Trevos | Todas as recompensas x1.5 | R$ 800 |
| Analista Profissional | 40% de chance de recompensa máxima | R$ 500 |
| Engenheiro | Máquinas trabalham 25% mais rápido | R$ 700 |

---

## 🏭 Máquinas

| Máquina | Função | Preço |
|---------|--------|-------|
| Lixo | Arraste uma pedra para descartá-la | R$ 50 |
| ↳ Lixo de Luxo | Upgrade: recebe 25% do valor de compra ao descartar | R$ 200 |
| Scanner | Contorno verde/vermelho em todas as pedras simultaneamente | R$ 150 |
| Quebrador Automático | Quebra pedras automaticamente (evoluível: 1→2→... dano/s) | R$ 300 |
| Inversor de Polaridade | Inverte o sinal do valor da pedra | R$ 400 |
| Loja de Cosméticos | Compre skins para suas pedras | R$ 500 |

---

## 💾 Sistema de Save

O jogo salva automaticamente a cada **60 segundos** e permite salvar manualmente pelo menu de pause. O arquivo fica em `Application.persistentDataPath/save.json` e guarda:

- Dinheiro atual
- Pedras na cena (tipo, posição, vida restante)
- Níveis de Força, Sorte e Fortuna
- Habilidades desbloqueadas
- Máquinas compradas

---

## 🎨 Prefabs

| Prefab | Uso |
|--------|-----|
| `Pedra2D` | Pedra instanciada na cena com física 2D |
| `PedraUI` | Versão UI da pedra (legado, não utilizado) |
| `ItemLoja` | Linha da lista de pedras na loja |
| `ItemStatus` | Linha da lista de status |
| `ItemMaquina` | Linha da lista de máquinas |
| `NoHabilidade` | Nó da árvore de habilidades |
| `TextoFlutuante` | Número de dano animado ao clicar |

---

## 🗺️ Hierarquia da Cena Jogo

```
Main Camera
EventSystem
GameManager
LojaManager
MaquinaManager + MaquinaDatabase
StatusManager
HabilidadeDatabase
SaveManager
PauseManager
GameLoader
ConfiguracoesManager
Computador
Mesa
MaquinaLixo
MaquinaScanner
MaquinaQuebrador
MaquinaInversor
Canvas
  ├── TextoDinheiro
  ├── ContainerDinheiro
  │     ├── TextoDinheiro
  │     └── TextoDiferenca
  ├── ContainerPedras
  ├── PainelLoja
  │     ├── BotaoFechar
  │     ├── BarraNavegacao
  │     │     ├── BotaoPedras
  │     │     ├── BotaoStatus
  │     │     ├── BotaoHabilidades
  │     │     └── BotaoMaquinas
  │     └── AreaConteudo
  │           ├── PainelPedras → ScrollView
  │           ├── PainelStatus → ScrollView
  │           ├── PainelHabilidades → AreaArvore
  │           └── PainelMaquinas → ScrollView
  ├── PainelPause
  ├── PainelConfiguracoes
  └── PainelTooltip
```

---

## 🗺️ Hierarquia da Cena MenuPrincipal

```
Main Camera
SaveManager
ConfiguracoesManager
MenuPrincipalManager
Canvas
  ├── PainelMenu
  │     ├── Titulo
  │     ├── BotaoNovoJogo
  │     ├── BotaoContinuar
  │     ├── BotaoConfiguracoes
  │     ├── BotaoCreditos
  │     └── BotaoSair
  └── PainelConfiguracoes
        ├── SliderVolumeGeral
        ├── SliderVolumeMusica
        ├── SliderVolumeEfeitos
        ├── DropdownResolucao
        ├── ToggleFullscreen
        ├── DropdownIdioma
        └── BotaoFechar
```

---

## 🚧 Pendente / Próximos Passos

- [ ] Sprites reais das pedras e máquinas
- [ ] Sprite e animação do personagem protagonista
- [ ] Sprite do minerador vendedor
- [ ] Tela de créditos
- [ ] Sistema de localização PT-BR / EN
- [ ] Tela de fim de jogo (R$ 1 bilhão)
- [ ] Mecânica de lixo integrada ao arrastar
- [ ] Loja de cosméticos funcional
- [ ] Sistema de sorteio de pedras preciosas ao quebrar
- [ ] Drone de transporte

---

*Desenvolvido por Mateus — ROCK IT © 2026*# rock-it
