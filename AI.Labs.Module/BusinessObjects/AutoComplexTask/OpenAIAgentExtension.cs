﻿using AutoGen.Core;
using AutoGen.OpenAI;

namespace AI.Labs.Module.BusinessObjects.AutoComplexTask
{
    public static class OpenAIAgentExtension
    {
        /// <summary>
        /// Register an <see cref="OpenAIChatRequestMessageConnector"/> to the <see cref="OpenAIChatAgent"/>
        /// </summary>
        /// <param name="connector">the connector to use. If null, a new instance of <see cref="OpenAIChatRequestMessageConnector"/> will be created.</param>
        public static MiddlewareStreamingAgent<OpenAIChatAgent> RegisterMessageConnector(
            this OpenAIChatAgent agent, OpenAIChatRequestMessageConnector? connector = null)
        {
            if (connector == null)
            {
                connector = new OpenAIChatRequestMessageConnector();
            }

            return agent.RegisterStreamingMiddleware(connector);
        }

        /// <summary>
        /// Register an <see cref="OpenAIChatRequestMessageConnector"/> to the <see cref="MiddlewareAgent{T}"/> where T is <see cref="OpenAIChatAgent"/>
        /// </summary>
        /// <param name="connector">the connector to use. If null, a new instance of <see cref="OpenAIChatRequestMessageConnector"/> will be created.</param>
        public static MiddlewareStreamingAgent<OpenAIChatAgent> RegisterMessageConnector(
            this MiddlewareStreamingAgent<OpenAIChatAgent> agent, OpenAIChatRequestMessageConnector? connector = null)
        {
            if (connector == null)
            {
                connector = new OpenAIChatRequestMessageConnector();
            }

            return agent.RegisterStreamingMiddleware(connector);
        }
    }

}
