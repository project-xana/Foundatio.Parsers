﻿using System;
using System.Collections.Generic;
using ElasticMacros.FilterMacros;
using ElasticMacros.QueryMacros;
using Exceptionless.LuceneQueryParser;
using Exceptionless.LuceneQueryParser.Nodes;
using Exceptionless.LuceneQueryParser.Visitor;
using Nest;

namespace ElasticMacros {
    public class ElasticMacroProcessor {
        private readonly List<IQueryNodeVisitorWithResult<IQueryNode>> _visitors = new List<IQueryNodeVisitorWithResult<IQueryNode>>();
        private readonly QueryParser _parser = new QueryParser();
        private readonly ElasticMacrosConfiguration _config;

        public ElasticMacroProcessor(Action<ElasticMacrosConfiguration> configure = null) {
            var config = new ElasticMacrosConfiguration();
            configure?.Invoke(config);
            _visitors.AddRange(config.Visitors);
            _config = config;
        }

        public FilterContainer BuildFilter(string query) {
            var result = _parser.Parse(query);

            for (int i = 0; i < _visitors.Count; i++)
                _visitors[i].Accept(result);

            return new FilterContainerVisitor(_config).Accept(result);
        }

        public QueryContainer BuildQuery(string query) {
            var result = _parser.Parse(query);

            for (int i = 0; i < _visitors.Count; i++)
                _visitors[i].Accept(result);

            return new QueryContainerVisitor(_config).Accept(result);
        }

        // parser query, generate filter, generate aggregations
        // want to be able to support things like date macro expansion (now-1d/d), geo query string filters, etc
        // date:"last 30 days"
        // number ranges field:1..
        // _exists_:field1
        // automatic field alias management
    }
}
