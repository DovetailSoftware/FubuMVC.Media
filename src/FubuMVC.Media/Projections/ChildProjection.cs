using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FubuCore.Reflection;

namespace FubuMVC.Media.Projections
{
    public class ChildProjection<TParent, TChild> : Projection<TChild>, IProjection<TParent> where TChild : class
    {
        private readonly Accessor _accessor;
        private string _name;
        private readonly Func<IProjectionContext<TParent>, TChild> _source;

        public ChildProjection(string name, Func<IProjectionContext<TParent>, TChild> source, DisplayFormatting formatting)
            : base(formatting)
        {
            _source = source;
            _name = name;
        }

        public ChildProjection(Expression<Func<TParent, TChild>> expression, DisplayFormatting formatting) : base(formatting)
        {
            _accessor = ReflectionHelper.GetAccessor(expression);
            _source = c => c.ValueFor(_accessor) as TChild;
            _name = _accessor.Name;
        }

        public ChildProjection<TParent, TChild> Name(string name)
        {
            _name = name;
            return this;
        }

        public ChildProjection<TParent, TChild> Configure(Action<Projection<TChild>> configuration)
        {
            configuration(this);
            return this;
        }

        public ChildProjection<TParent, TChild> With(Action<IProjectionContext<TChild>, IMediaNode> explicitWriting)
        {
            return Configure(x => x.WriteWith(explicitWriting));
        }

        public ChildProjection<TParent, TChild> With<TProjection>() where TProjection : IProjection<TChild>, new()
        {
            Include<TProjection>();
            return this;
        }

        IEnumerable<Accessor> IProjection<TParent>.Accessors()
        {
            if (_accessor != null) yield return _accessor;
        }

        void IProjection<TParent>.Write(IProjectionContext<TParent> context, IMediaNode node)
        {
            var value = _source(context);
            if (value == null) return;

            var childNode = node.AddChild(_name);

            var childContext = context.ContextFor(value);

            write(childContext, childNode);
        }
    }
}