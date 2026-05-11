using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using LogicBuilder.Data;
using LogicBuilder.Domain;
using LogicBuilder.EntityFrameworkCore.SqlServer.Crud.DataStores;
using LogicBuilder.EntityFrameworkCore.SqlServer.Visitors;
using LogicBuilder.Expressions.Utils;
using LogicBuilder.Expressions.Utils.Expansions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.Repositories
{
    internal static class StoreHelpers
    {
        internal static async Task<ICollection<TModel>> GetAsync<TModel, TData>(this IStore store, IMapper mapper,
            Expression<Func<TModel, bool>>? filter = null,
            Expression<Func<IQueryable<TModel>, IQueryable<TModel>>>? queryFunc = null,
            SelectExpandDefinition? selectExpandDefinition = null)
            where TModel : class, IBaseModel
            where TData : class, IBaseData
        {
            return [.. mapper.ProjectTo
            (
                await store.GetQueryableAsync
                (
                    Getfilter(),
                    GetQueryFunc()
                ),
                null,
                GetIncludes<TModel>(selectExpandDefinition)
            )
            .UpdateQueryable(selectExpandDefinition!.GetExpansions(typeof(TModel)), mapper)];//GetExpansions returns empty list if selectExpandDefinition is null

            Func<IQueryable<TData>, IQueryable<TData>>? GetQueryFunc()
                => mapper.MapExpression<Expression<Func<IQueryable<TData>, IQueryable<TData>>>>(queryFunc)?.Compile();

            Expression<Func<TData, bool>> Getfilter()
                => mapper.MapExpression<Expression<Func<TData, bool>>>(filter);
        }

        private static Expression<Func<TModel, object>>[] GetIncludes<TModel>(SelectExpandDefinition? selectExpandDefinition) where TModel : class
                => [.. selectExpandDefinition!.GetExpansionSelectors<TModel>()];//GetExpansionSelectors returns empty list if selectExpandDefinition is null

        internal static async Task<TModelReturn> QueryAsync<TModel, TData, TModelReturn, TDataReturn>(this IStore store, IMapper mapper,//NOSONAR -  in this case, reducing the number of generic parameters adds more complexity and would not be beneficial to readability.
            Expression<Func<IQueryable<TModel>, TModelReturn>> queryFunc,
            SelectExpandDefinition? selectExpandDefinition = null)
            where TModel : class, IBaseModel
            where TData : class, IBaseData
        {
            //Map the expressions
            Expression<Func<IQueryable<TData>, TDataReturn>> mappedQueryFunc = mapper.MapExpression<Expression<Func<IQueryable<TData>, TDataReturn>>>(queryFunc);

            //Call the store
            TDataReturn result = await store.QueryAsync(mappedQueryFunc.Compile());

            if (typeof(TModelReturn) == typeof(TDataReturn))
                return (TModelReturn)(object)result!;
            else if (typeof(TModelReturn).IsIQueryable() && typeof(TDataReturn).IsIQueryable())
                return GetProjection(typeof(TModelReturn).GetUnderlyingElementType());

            return mapper.Map<TDataReturn, TModelReturn>(result);

            TModelReturn GetProjection(Type elementType)
                => (TModelReturn)mapper.ProjectTo
                (
                    (IQueryable)result!,
                    elementType,
                    selectExpandDefinition
                )
                .UpdateQueryable(elementType, selectExpandDefinition!.GetExpansions(elementType), mapper);//GetExpansions returns empty list if selectExpandDefinition is null
        }

        public static IQueryable ProjectTo(this IMapper mapper, IQueryable source, Type destType, SelectExpandDefinition? selectExpandDefinition = null)
            => (nameof(ProjectTo).GetGenericMethodInfo().MakeGenericMethod
            (
                destType
            ).Invoke(null, [mapper, source, selectExpandDefinition]) as IQueryable)!;//ProjectTo is guaranteed to return an IQueryable, so the null forgiving operator can be used here.

        private static IQueryable<TDest> ProjectTo<TDest>(IMapper mapper, IQueryable source, SelectExpandDefinition? selectExpandDefinition = null) where TDest : class
            => mapper.ProjectTo<TDest>(source, null, GetIncludes<TDest>(selectExpandDefinition));

        private static MethodInfo GetGenericMethodInfo(this string methodName)
           => typeof(StoreHelpers).GetMethods
            (
               BindingFlags.NonPublic// NOSONAR accessing a genric method via reflection is necessary in this case
               | BindingFlags.Static
            ).Single(m => m.Name == methodName && m.IsGenericMethod);

        internal static Task<int> CountAsync<TModel, TData>(this IStore store, IMapper mapper, Expression<Func<TModel, bool>>? filter = null)
            where TModel : IBaseModel
            where TData : class, IBaseData
        {
            Expression<Func<TData, bool>> f = mapper.MapExpression<Expression<Func<TData, bool>>>(filter);
            return store.CountAsync(f);
        }

        internal static async Task<bool> SaveGraphsAsync<TModel, TData>(this IStore store, IMapper mapper, ICollection<TModel> entities)
            where TModel : IBaseModel
            where TData : class, IBaseData
        {
            List<TData> items = [.. mapper.Map<IEnumerable<TData>>(entities)];
            bool success = await store.SaveGraphsAsync<TData>(items);

            List<TModel> entityList = [.. entities];
            for (int i = 0; i < items.Count; i++)
                mapper.Map(items[i], entityList[i]);

            return success;
        }

        internal static async Task<bool> SaveAsync<TModel, TData>(this IStore store, IMapper mapper, ICollection<TModel> entities)
            where TModel : IBaseModel
            where TData : class, IBaseData
        {
            List<TData> items = [.. mapper.Map<IEnumerable<TData>>(entities)];
            bool success = await store.SaveAsync(items);

            List<TModel> entityList = [.. entities];
            for (int i = 0; i < items.Count; i++)
                mapper.Map(items[i], entityList[i]);

            return success;
        }

        internal static async Task<bool> DeleteAsync<TModel, TData>(this IStore store, IMapper mapper, Expression<Func<TModel, bool>>? filter = null)
            where TModel : IBaseModel
            where TData : class, IBaseData
        {
            Expression<Func<TData, bool>> f = mapper.MapExpression<Expression<Func<TData, bool>>>(filter);
            List<TData> list = [.. (await store.GetAsync(f))];
            list.ForEach(item => { item.EntityState = Data.EntityStateType.Deleted; });
            return await store.SaveAsync<TData>(list);
        }

        internal static void AddChanges<TModel, TData>(this IStore store, IMapper mapper, ICollection<TModel> entities)
            where TModel : IBaseModel
            where TData : class, IBaseData
        {
            store.AddChanges([.. mapper.Map<IEnumerable<TData>>(entities)]);
        }

        internal static void AddGraphChanges<TModel, TData>(this IStore store, IMapper mapper, ICollection<TModel> entities)
            where TModel : IBaseModel
            where TData : class, IBaseData
        {
            store.AddGraphChanges([.. mapper.Map<IEnumerable<TData>>(entities)]);
        }

        internal static IQueryable UpdateQueryable(this IQueryable query, Type modelType, List<List<ExpansionOptions>> expansions, IMapper mapper) 
            => (nameof(UpdateQueryable).GetGenericMethodInfo().MakeGenericMethod
            (
                modelType
            ).Invoke(null, [query, expansions, mapper])  as IQueryable)!;//UpdateQueryable is guaranteed to return an IQueryable, so the null forgiving operator can be used here.

        internal static IQueryable<TModel> UpdateQueryable<TModel>(this IQueryable<TModel> query, List<List<ExpansionOptions>> expansions, IMapper mapper)
        {
            List<List<ExpansionOptions>> filters = GetFilters(expansions);
            List<List<ExpansionOptions>> methods = GetQueryMethods(expansions);

            if (filters.Count == 0 && methods.Count == 0)
                return query;

            Expression expression = query.Expression;

            if (methods.Count != 0)
                expression = UpdateProjectionMethodExpression(expression);

            if (filters.Count != 0)
                expression = UpdateProjectionFilterExpression(expression);

            return query.Provider.CreateQuery<TModel>(expression);

            Expression UpdateProjectionFilterExpression(Expression projectionExpression)
            {
                filters.ForEach
                (
                    filterList => projectionExpression = FilterUpdater.UpdaterExpansion
                    (
                        projectionExpression,
                        filterList,
                        mapper
                    )
                );

                return projectionExpression;
            }

            Expression UpdateProjectionMethodExpression(Expression projectionExpression)
            {
                methods.ForEach
                (
                    methodList => projectionExpression = QueryFunctionUpdater.UpdaterExpansion
                    (
                        projectionExpression,
                        methodList
                    )
                );

                return projectionExpression;
            }

            static List<List<ExpansionOptions>> GetFilters(List<List<ExpansionOptions>> expansions)
                => expansions.Aggregate(new List<List<ExpansionOptions>>(), (listOfLists, nextList) =>
                {
                    var filterNextList = new List<ExpansionOptions>();
                    foreach (ExpansionOptions next in nextList)
                    {
                        if (next.FilterOption?.FilterLambdaOperator != null)
                        {
                            filterNextList = filterNextList.ConvertAll
                            (
                                exp => new ExpansionOptions
                                (
                                    exp.MemberName,
                                    exp.MemberType,
                                    exp.ParentType,
                                    []
                                )
                            );//new list removing filter

                            filterNextList.Add
                            (
                                new ExpansionOptions
                                (
                                    next.MemberName,
                                    next.MemberType,
                                    next.ParentType,
                                    [],
                                    null,
                                    new ExpansionFilterOption(next.FilterOption.FilterLambdaOperator)
                                )
                            );//add expansion with filter

                            listOfLists.Add([.. filterNextList]); //Add the whole list to the list of filter lists
                                                                      //Only the last item in each list has a filter
                                                                      //Filters for parent expansions exist in their own lists
                            continue;
                        }

                        filterNextList.Add(next);

                    }

                    return listOfLists;
                });

            static List<List<ExpansionOptions>> GetQueryMethods(List<List<ExpansionOptions>> expansions)
                => expansions.Aggregate(new List<List<ExpansionOptions>>(), (listOfLists, nextList) =>
                {
                    var queryMethodNextList = new List<ExpansionOptions>();

                    foreach (ExpansionOptions next in nextList)
                    {
                        if (next.QueryOption?.SortCollection != null)
                        {
                            queryMethodNextList = queryMethodNextList.ConvertAll
                            (
                                exp => new ExpansionOptions
                                (
                                    exp.MemberName,
                                    exp.MemberType,
                                    exp.ParentType,
                                    []
                                )
                            );//new list removing query options

                            queryMethodNextList.Add
                            (
                                new ExpansionOptions
                                (
                                    next.MemberName,
                                    next.MemberType,
                                    next.ParentType,
                                    [],
                                    new ExpansionQueryOption(next.QueryOption.SortCollection)
                                )
                            );//add expansion with query options

                            listOfLists.Add([.. queryMethodNextList]); //Add the whole list to the list of query method lists
                                                                           //Only the last item in each list has a query method
                                                                           //Query methods for parent expansions exist in their own lists
                            continue;
                        }

                        queryMethodNextList.Add(next);
                    }

                    return listOfLists;
                });
        }
    }
}
