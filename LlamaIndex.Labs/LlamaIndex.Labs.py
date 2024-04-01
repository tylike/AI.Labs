from llama_index.llms import OpenAI,OpenAILike
from llama_index import OpenAIEmbedding, VectorStoreIndex, SimpleDirectoryReader, ServiceContext
import os

os.environ['OPENAI_API_KEY '] = '111'

#llm = OpenAI(temperature=0.1, model="gpt-4",api_base="http://127.0.0.1:8000",api_key="111")
llm = OpenAILike(
    model="localhost",
    api_base="http://localhost:8000/v1",
    api_key="fake",
    api_type="fake",
    max_tokens=256,
    temperatue=0.5,
)


embed_model = OpenAIEmbedding(api_base="http://localhost:8000/v1",api_key="111")

service_context = ServiceContext.from_defaults(llm=llm,embed_model=embed_model)

documents = SimpleDirectoryReader("data").load_data()
index = VectorStoreIndex.from_documents(
    documents, service_context=service_context
)
print(index)

