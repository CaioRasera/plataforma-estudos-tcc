# RevisIA - Plataforma Adaptativa de Estudos

Este repositório contém o código-fonte do TCC: **Plataforma Adaptativa de Estudos com Geração Automática de Avaliações e Repetição Espaçada baseada em IA Generativa**.

## 🚀 Sobre o Projeto
A **RevisIA** é uma plataforma SaaS educacional desenvolvida para maximizar a retenção de conhecimento. O sistema permite o upload de materiais de estudo em PDF e utiliza Inteligência Artificial Generativa (Llama 3 / Groq) para extrair conceitos-chave e gerar automaticamente Decks de Flashcards. 

Através de um algoritmo otimizado de **Repetição Espaçada**, a plataforma agenda as revisões do usuário nos momentos ideais para evitar a curva de esquecimento.

## 🛠️ Tecnologias Utilizadas
- **Frontend:** React (Vite), TypeScript, TailwindCSS, Recharts.
- **Backend:** C# .NET 8, Entity Framework Core.
- **Banco de Dados:** PostgreSQL (Supabase).
- **Inteligência Artificial:** Integração via API (GroqProvider) com Llama3/GPT para processamento de linguagem natural e chunking inteligente de documentos.

## ⚙️ Como executar localmente
Como as chaves de API e conexão de banco de dados não são versionadas por segurança, você precisará configurar suas variáveis locais.

1. Clone o repositório.
2. Na pasta ackend/StudyPlatform.API, crie o arquivo ppsettings.Development.json baseado nas chaves necessárias.
3. Inicie o Backend: dotnet run (porta 5245)
4. Inicie o Frontend: 
pm install e 
pm run dev (porta 5173)

## 👤 Autor
Desenvolvido como projeto de Trabalho de Conclusão de Curso (TCC).