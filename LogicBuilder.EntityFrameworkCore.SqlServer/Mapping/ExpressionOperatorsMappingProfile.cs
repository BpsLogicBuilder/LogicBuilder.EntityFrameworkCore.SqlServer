using AutoMapper;
using LogicBuilder.Expressions.Utils.ExpressionBuilder;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Arithmetic;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Cacnonical;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Collection;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Conversions;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.DateTimeOperators;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Lambda;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Logical;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Operand;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.StringOperators;
using LogicBuilder.Expressions.Utils.ExpressionDescriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.Mapping
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class ExpressionOperatorsMappingProfile : Profile
	{
		public ExpressionOperatorsMappingProfile()
		{
			CreateMap<AddBinaryDescriptor, AddBinaryOperator>();
			CreateMap<AllDescriptor, AllOperator>()
				.ConstructUsing
				(
					(src, context) => new AllOperator
					(
						(IDictionary<string, ParameterExpression>)context.Items[ExpressionOperators.PARAMETERS_KEY],
						context.Mapper.Map<IExpressionPart>(src.SourceOperand),
						context.Mapper.Map<IExpressionPart>(src.FilterBody),
						src.FilterParameterName
					)
				)
				.ForAllMembers(opt => opt.Ignore());

			CreateMap<AndBinaryDescriptor, AndBinaryOperator>();
			CreateMap<AnyDescriptor, AnyOperator>()
				.ConstructUsing
				(
					(src, context) => new AnyOperator
					(
						(IDictionary<string, ParameterExpression>)context.Items[ExpressionOperators.PARAMETERS_KEY],
						context.Mapper.Map<IExpressionPart>(src.SourceOperand),
						context.Mapper.Map<IExpressionPart>(src.FilterBody),
						src.FilterParameterName
					)
				)
				.ForAllMembers(opt => opt.Ignore());

			CreateMap<AsEnumerableDescriptor, AsEnumerableOperator>();
			CreateMap<AsQueryableDescriptor, AsQueryableOperator>();
			CreateMap<AverageDescriptor, AverageOperator>()
				.ConstructUsing
				(
					(src, context) => new AverageOperator
					(
						(IDictionary<string, ParameterExpression>)context.Items[ExpressionOperators.PARAMETERS_KEY],
						context.Mapper.Map<IExpressionPart>(src.SourceOperand),
						context.Mapper.Map<IExpressionPart>(src.SelectorBody),
						src.SelectorParameterName
					)
				)
				.ForAllMembers(opt => opt.Ignore());

			CreateMap<BinaryDescriptor, BinaryOperator>();
			CreateMap<CastDescriptor, CastOperator>()
                .ForMember(dest => dest.Type, opts => opts.Ignore())
                .ForCtorParam("type", opts => opts.MapFrom(x => Type.GetType(x.Type)));
            CreateMap<CeilingDescriptor, CeilingOperator>();
			CreateMap<CollectionCastDescriptor, CollectionCastOperator>()
				.ForMember(dest => dest.Type, opts => opts.Ignore())
                .ForCtorParam("type", opts => opts.MapFrom(x => Type.GetType(x.Type)));
            CreateMap<CollectionConstantDescriptor, CollectionConstantOperator>()
                .ForMember(dest => dest.ElementType, opts => opts.Ignore())
                .ForCtorParam("elementType", opts => opts.MapFrom(x => Type.GetType(x.ElementType)));
			CreateMap<CollectionOfTypeDescriptor, CollectionOfTypeOperator>()
				.ForMember(dest => dest.Type, opts => opts.Ignore())
				.ForCtorParam("type", opts => opts.MapFrom(x => Type.GetType(x.Type)));
			CreateMap<ConcatDescriptor, ConcatOperator>();
			CreateMap<ConstantDescriptor, ConstantOperator>()
				.ForMember(dest => dest.Type, opts => opts.Ignore())
                .ForCtorParam("type", opts => opts.MapFrom(x => Type.GetType(x.Type ?? "")));
            CreateMap<ContainsDescriptor, ContainsOperator>();
			CreateMap<ConvertCharArrayToStringDescriptor, ConvertCharArrayToStringOperator>();
			CreateMap<ConvertDescriptor, ConvertOperator>()
                .ForMember(dest => dest.Type, opts => opts.Ignore())
                .ForCtorParam("type", opts => opts.MapFrom(x => Type.GetType(x.Type)));
            CreateMap<ConvertToEnumDescriptor, ConvertToEnumOperator>()
                .ForMember(dest => dest.Type, opts => opts.Ignore())
                .ForCtorParam("type", opts => opts.MapFrom(x => Type.GetType(x.Type)));
            CreateMap<ConvertToNullableUnderlyingValueDescriptor, ConvertToNullableUnderlyingValueOperator>();
			CreateMap<ConvertToNumericDateDescriptor, ConvertToNumericDateOperator>();
			CreateMap<ConvertToNumericTimeDescriptor, ConvertToNumericTimeOperator>();
			CreateMap<ConvertToStringDescriptor, ConvertToStringOperator>();
			CreateMap<CountDescriptor, CountOperator>()
				.ConstructUsing
				(
					(src, context) => new CountOperator
					(
						(IDictionary<string, ParameterExpression>)context.Items[ExpressionOperators.PARAMETERS_KEY],
						context.Mapper.Map<IExpressionPart>(src.SourceOperand),
						context.Mapper.Map<IExpressionPart>(src.FilterBody),
						src.FilterParameterName
					)
				)
				.ForAllMembers(opt => opt.Ignore());
            CreateMap<CustomMethodDescriptor, CustomMethodOperator>()
                .ForMember(dest => dest.MethodInfo, opts => opts.Ignore())
                .ForCtorParam
                (
                    "methodInfo",
                    opts => opts.MapFrom
                    (
                        x => Type.GetType(x.DeclaringType)!.GetMethod
                        (
                            x.MethodName,
                            BindingFlags.NonPublic //NOSONAR - The method may be public or non-public. See test case CustomMethod_StaticMethoOfDeclaringType
                                | BindingFlags.Instance
                                | BindingFlags.Public
                                | BindingFlags.Static,
                            x.ParameterTypeNames.Select(tn => Type.GetType(tn)!).ToArray()
                        )
                    )
                );
            CreateMap<DateDescriptor, DateOperator>();
			CreateMap<DayDescriptor, DayOperator>();
			CreateMap<DistinctDescriptor, DistinctOperator>();
			CreateMap<DivideBinaryDescriptor, DivideBinaryOperator>();
			CreateMap<EndsWithDescriptor, EndsWithOperator>();
			CreateMap<EqualsBinaryDescriptor, EqualsBinaryOperator>();
            CreateMap<ExceptDescriptor, ExceptOperator>();
            CreateMap<FilterLambdaDescriptor, FilterLambdaOperator>()
				.ConstructUsing
				(
					(src, context) => new FilterLambdaOperator
					(
						(IDictionary<string, ParameterExpression>)context.Items[ExpressionOperators.PARAMETERS_KEY],
						context.Mapper.Map<IExpressionPart>(src.FilterBody),
                        Type.GetType(src.SourceElementType) ?? throw new InvalidOperationException("SourceElementType cannot be null"),
						src.ParameterName
					)
				)
				.ForAllMembers(opt => opt.Ignore());

			CreateMap<FirstDescriptor, FirstOperator>()
				.ConstructUsing
				(
					(src, context) => new FirstOperator
					(
						(IDictionary<string, ParameterExpression>)context.Items[ExpressionOperators.PARAMETERS_KEY],
						context.Mapper.Map<IExpressionPart>(src.SourceOperand),
						context.Mapper.Map<IExpressionPart>(src.FilterBody),
						src.FilterParameterName
					)
				)
				.ForAllMembers(opt => opt.Ignore());

			CreateMap<FirstOrDefaultDescriptor, FirstOrDefaultOperator>()
				.ConstructUsing
				(
					(src, context) => new FirstOrDefaultOperator
					(
						(IDictionary<string, ParameterExpression>)context.Items[ExpressionOperators.PARAMETERS_KEY],
						context.Mapper.Map<IExpressionPart>(src.SourceOperand),
						context.Mapper.Map<IExpressionPart>(src.FilterBody),
						src.FilterParameterName
					)
				)
				.ForAllMembers(opt => opt.Ignore());

			CreateMap<FloorDescriptor, FloorOperator>();
			CreateMap<FractionalSecondsDescriptor, FractionalSecondsOperator>();
			CreateMap<GreaterThanBinaryDescriptor, GreaterThanBinaryOperator>();
			CreateMap<GreaterThanOrEqualsBinaryDescriptor, GreaterThanOrEqualsBinaryOperator>();
			CreateMap<GroupByDescriptor, GroupByOperator>()
				.ConstructUsing
				(
					(src, context) => new GroupByOperator
					(
						(IDictionary<string, ParameterExpression>)context.Items[ExpressionOperators.PARAMETERS_KEY],
						context.Mapper.Map<IExpressionPart>(src.SourceOperand),
						context.Mapper.Map<IExpressionPart>(src.SelectorBody),
						src.SelectorParameterName
					)
				)
				.ForAllMembers(opt => opt.Ignore());

			CreateMap<HasDescriptor, HasOperator>();
			CreateMap<HourDescriptor, HourOperator>();
			CreateMap<EnumerableSelectorLambdaDescriptor, EnumerableSelectorLambdaOperator>()
				.ConstructUsing
				(
					(src, context) => new EnumerableSelectorLambdaOperator
					(
						(IDictionary<string, ParameterExpression>)context.Items[ExpressionOperators.PARAMETERS_KEY],
						context.Mapper.Map<IExpressionPart>(src.Selector),
                        Type.GetType(src.SourceElementType) ?? throw new InvalidOperationException("SourceElementType cannot be null"),
						src.ParameterName
					)
				)
				.ForAllMembers(opt => opt.Ignore());

			CreateMap<IndexOfDescriptor, IndexOfOperator>();
			CreateMap<InDescriptor, InOperator>();
			CreateMap<IsOfDescriptor, IsOfOperator>()
                .ForMember(dest => dest.Type, opts => opts.Ignore())
                .ForCtorParam("type", opts => opts.MapFrom(x => Type.GetType(x.Type)));
            CreateMap<LastDescriptor, LastOperator>()
				.ConstructUsing
				(
					(src, context) => new LastOperator
					(
						(IDictionary<string, ParameterExpression>)context.Items[ExpressionOperators.PARAMETERS_KEY],
						context.Mapper.Map<IExpressionPart>(src.SourceOperand),
						context.Mapper.Map<IExpressionPart>(src.FilterBody),
						src.FilterParameterName
					)
				)
				.ForAllMembers(opt => opt.Ignore());

			CreateMap<LastOrDefaultDescriptor, LastOrDefaultOperator>()
				.ConstructUsing
				(
					(src, context) => new LastOrDefaultOperator
					(
						(IDictionary<string, ParameterExpression>)context.Items[ExpressionOperators.PARAMETERS_KEY],
						context.Mapper.Map<IExpressionPart>(src.SourceOperand),
						context.Mapper.Map<IExpressionPart>(src.FilterBody),
						src.FilterParameterName
					)
				)
				.ForAllMembers(opt => opt.Ignore());

			CreateMap<LengthDescriptor, LengthOperator>();
			CreateMap<LessThanBinaryDescriptor, LessThanBinaryOperator>();
			CreateMap<LessThanOrEqualsBinaryDescriptor, LessThanOrEqualsBinaryOperator>();
			CreateMap<MaxDateTimeDescriptor, MaxDateTimeOperator>();
			CreateMap<MaxDescriptor, MaxOperator>()
				.ConstructUsing
				(
					(src, context) => new MaxOperator
					(
						(IDictionary<string, ParameterExpression>)context.Items[ExpressionOperators.PARAMETERS_KEY],
						context.Mapper.Map<IExpressionPart>(src.SourceOperand),
						context.Mapper.Map<IExpressionPart>(src.SelectorBody),
						src.SelectorParameterName
					)
				)
				.ForAllMembers(opt => opt.Ignore());

			CreateMap<MemberInitDescriptor, MemberInitOperator>()
                .ForMember(dest => dest.NewType, opts => opts.Ignore())
                .ForCtorParam("newType", opts => opts.MapFrom(x => Type.GetType(x.NewType ?? "")));
            CreateMap<MemberSelectorDescriptor, MemberSelectorOperator>();
			CreateMap<MinDateTimeDescriptor, MinDateTimeOperator>();
			CreateMap<MinDescriptor, MinOperator>()
				.ConstructUsing
				(
					(src, context) => new MinOperator
					(
						(IDictionary<string, ParameterExpression>)context.Items[ExpressionOperators.PARAMETERS_KEY],
						context.Mapper.Map<IExpressionPart>(src.SourceOperand),
						context.Mapper.Map<IExpressionPart>(src.SelectorBody),
						src.SelectorParameterName
					)
				)
				.ForAllMembers(opt => opt.Ignore());

			CreateMap<MinuteDescriptor, MinuteOperator>();
			CreateMap<ModuloBinaryDescriptor, ModuloBinaryOperator>();
			CreateMap<MonthDescriptor, MonthOperator>();
			CreateMap<MultiplyBinaryDescriptor, MultiplyBinaryOperator>();
			CreateMap<NegateDescriptor, NegateOperator>();
			CreateMap<NotEqualsBinaryDescriptor, NotEqualsBinaryOperator>();
			CreateMap<NotDescriptor, NotOperator>();
			CreateMap<NowDateTimeDescriptor, NowDateTimeOperator>();
			CreateMap<OrBinaryDescriptor, OrBinaryOperator>();
			CreateMap<OrderByDescriptor, OrderByOperator>()
				.ConstructUsing
				(
					(src, context) => new OrderByOperator
					(
						(IDictionary<string, ParameterExpression>)context.Items[ExpressionOperators.PARAMETERS_KEY],
						context.Mapper.Map<IExpressionPart>(src.SourceOperand),
						context.Mapper.Map<IExpressionPart>(src.SelectorBody),
						src.SortDirection,
						src.SelectorParameterName
					)
				)
				.ForAllMembers(opt => opt.Ignore());

			CreateMap<ParameterDescriptor, ParameterOperator>()
				.ConstructUsing
				(
					(src, context) => new ParameterOperator
					(
						(IDictionary<string, ParameterExpression>)context.Items[ExpressionOperators.PARAMETERS_KEY],
						src.ParameterName
					)
				)
				.ForAllMembers(opt => opt.Ignore());

			CreateMap<RoundDescriptor, RoundOperator>();
			CreateMap<SecondDescriptor, SecondOperator>();
			CreateMap<SelectManyDescriptor, SelectManyOperator>()
				.ConstructUsing
				(
					(src, context) => new SelectManyOperator
					(
						(IDictionary<string, ParameterExpression>)context.Items[ExpressionOperators.PARAMETERS_KEY],
						context.Mapper.Map<IExpressionPart>(src.SourceOperand),
						context.Mapper.Map<IExpressionPart>(src.SelectorBody),
						src.SelectorParameterName
					)
				)
				.ForAllMembers(opt => opt.Ignore());

			CreateMap<SelectDescriptor, SelectOperator>()
				.ConstructUsing
				(
					(src, context) => new SelectOperator
					(
						(IDictionary<string, ParameterExpression>)context.Items[ExpressionOperators.PARAMETERS_KEY],
						context.Mapper.Map<IExpressionPart>(src.SourceOperand),
						context.Mapper.Map<IExpressionPart>(src.SelectorBody),
						src.SelectorParameterName
					)
				)
				.ForAllMembers(opt => opt.Ignore());

			CreateMap<SelectorLambdaDescriptor, SelectorLambdaOperator>()
				.ConstructUsing
				(
					(src, context) => new SelectorLambdaOperator
					(
						(IDictionary<string, ParameterExpression>)context.Items[ExpressionOperators.PARAMETERS_KEY],
						context.Mapper.Map<IExpressionPart>(src.Selector),
                        Type.GetType(src.SourceElementType) ?? throw new InvalidOperationException("SourceElementType cannot be null"),
                        Type.GetType(src.BodyType ?? "")!,//SelectorLambdaOperator handles null BodyType as object
                        src.ParameterName
					)
				)
				.ForAllMembers(opt => opt.Ignore());

			CreateMap<SingleDescriptor, SingleOperator>()
				.ConstructUsing
				(
					(src, context) => new SingleOperator
					(
						(IDictionary<string, ParameterExpression>)context.Items[ExpressionOperators.PARAMETERS_KEY],
						context.Mapper.Map<IExpressionPart>(src.SourceOperand),
						context.Mapper.Map<IExpressionPart>(src.FilterBody),
						src.FilterParameterName
					)
				)
				.ForAllMembers(opt => opt.Ignore());

			CreateMap<SingleOrDefaultDescriptor, SingleOrDefaultOperator>()
				.ConstructUsing
				(
					(src, context) => new SingleOrDefaultOperator
					(
						(IDictionary<string, ParameterExpression>)context.Items[ExpressionOperators.PARAMETERS_KEY],
						context.Mapper.Map<IExpressionPart>(src.SourceOperand),
						context.Mapper.Map<IExpressionPart>(src.FilterBody),
						src.FilterParameterName
					)
				)
				.ForAllMembers(opt => opt.Ignore());

			CreateMap<SkipDescriptor, SkipOperator>();
			CreateMap<StartsWithDescriptor, StartsWithOperator>();
			CreateMap<SubstringDescriptor, SubstringOperator>();
			CreateMap<SubtractBinaryDescriptor, SubtractBinaryOperator>();
			CreateMap<SumDescriptor, SumOperator>()
				.ConstructUsing
				(
					(src, context) => new SumOperator
					(
						(IDictionary<string, ParameterExpression>)context.Items[ExpressionOperators.PARAMETERS_KEY],
						context.Mapper.Map<IExpressionPart>(src.SourceOperand),
						context.Mapper.Map<IExpressionPart>(src.SelectorBody),
						src.SelectorParameterName
					)
				)
				.ForAllMembers(opt => opt.Ignore());

			CreateMap<TakeDescriptor, TakeOperator>();
			CreateMap<ThenByDescriptor, ThenByOperator>()
				.ConstructUsing
				(
					(src, context) => new ThenByOperator
					(
						(IDictionary<string, ParameterExpression>)context.Items[ExpressionOperators.PARAMETERS_KEY],
						context.Mapper.Map<IExpressionPart>(src.SourceOperand),
						context.Mapper.Map<IExpressionPart>(src.SelectorBody),
						src.SortDirection,
						src.SelectorParameterName
					)
				)
				.ForAllMembers(opt => opt.Ignore());

			CreateMap<TimeDescriptor, TimeOperator>();
			CreateMap<ToListDescriptor, ToListOperator>();
			CreateMap<ToLowerDescriptor, ToLowerOperator>();
			CreateMap<TotalOffsetMinutesDescriptor, TotalOffsetMinutesOperator>();
			CreateMap<TotalSecondsDescriptor, TotalSecondsOperator>();
			CreateMap<ToUpperDescriptor, ToUpperOperator>();
			CreateMap<TrimDescriptor, TrimOperator>();
            CreateMap<UnionDescriptor, UnionOperator>();
            CreateMap<WhereDescriptor, WhereOperator>()
				.ConstructUsing
				(
					(src, context) => new WhereOperator
					(
						(IDictionary<string, ParameterExpression>)context.Items[ExpressionOperators.PARAMETERS_KEY],
						context.Mapper.Map<IExpressionPart>(src.SourceOperand),
						context.Mapper.Map<IExpressionPart>(src.FilterBody),
						src.FilterParameterName
					)
				)
				.ForAllMembers(opt => opt.Ignore());

			CreateMap<YearDescriptor, YearOperator>();

			CreateMap<IExpressionDescriptor, IExpressionPart>()
				.Include<AddBinaryDescriptor, AddBinaryOperator>()
				.Include<AllDescriptor, AllOperator>()
				.Include<AndBinaryDescriptor, AndBinaryOperator>()
				.Include<AnyDescriptor, AnyOperator>()
				.Include<AsEnumerableDescriptor, AsEnumerableOperator>()
				.Include<AsQueryableDescriptor, AsQueryableOperator>()
				.Include<AverageDescriptor, AverageOperator>()
				.Include<BinaryDescriptor, BinaryOperator>()
				.Include<CastDescriptor, CastOperator>()
				.Include<CeilingDescriptor, CeilingOperator>()
				.Include<CollectionCastDescriptor, CollectionCastOperator>()
				.Include<CollectionConstantDescriptor, CollectionConstantOperator>()
				.Include<ConcatDescriptor, ConcatOperator>()
				.Include<ConstantDescriptor, ConstantOperator>()
				.Include<ContainsDescriptor, ContainsOperator>()
				.Include<ConvertCharArrayToStringDescriptor, ConvertCharArrayToStringOperator>()
				.Include<ConvertDescriptor, ConvertOperator>()
				.Include<ConvertToEnumDescriptor, ConvertToEnumOperator>()
				.Include<ConvertToNullableUnderlyingValueDescriptor, ConvertToNullableUnderlyingValueOperator>()
				.Include<ConvertToNumericDateDescriptor, ConvertToNumericDateOperator>()
				.Include<ConvertToNumericTimeDescriptor, ConvertToNumericTimeOperator>()
				.Include<ConvertToStringDescriptor, ConvertToStringOperator>()
				.Include<CountDescriptor, CountOperator>()
				.Include<CustomMethodDescriptor, CustomMethodOperator>()
				.Include<DateDescriptor, DateOperator>()
				.Include<DayDescriptor, DayOperator>()
				.Include<DistinctDescriptor, DistinctOperator>()
				.Include<DivideBinaryDescriptor, DivideBinaryOperator>()
				.Include<EndsWithDescriptor, EndsWithOperator>()
				.Include<EqualsBinaryDescriptor, EqualsBinaryOperator>()
                .Include<ExceptDescriptor, ExceptOperator>()
                .Include<FilterLambdaDescriptor, FilterLambdaOperator>()
				.Include<FirstDescriptor, FirstOperator>()
				.Include<FirstOrDefaultDescriptor, FirstOrDefaultOperator>()
				.Include<FloorDescriptor, FloorOperator>()
				.Include<FractionalSecondsDescriptor, FractionalSecondsOperator>()
				.Include<GreaterThanBinaryDescriptor, GreaterThanBinaryOperator>()
				.Include<GreaterThanOrEqualsBinaryDescriptor, GreaterThanOrEqualsBinaryOperator>()
				.Include<GroupByDescriptor, GroupByOperator>()
				.Include<HasDescriptor, HasOperator>()
				.Include<HourDescriptor, HourOperator>()
				.Include<EnumerableSelectorLambdaDescriptor, EnumerableSelectorLambdaOperator>()
				.Include<IndexOfDescriptor, IndexOfOperator>()
				.Include<InDescriptor, InOperator>()
				.Include<IsOfDescriptor, IsOfOperator>()
				.Include<LastDescriptor, LastOperator>()
				.Include<LastOrDefaultDescriptor, LastOrDefaultOperator>()
				.Include<LengthDescriptor, LengthOperator>()
				.Include<LessThanBinaryDescriptor, LessThanBinaryOperator>()
				.Include<LessThanOrEqualsBinaryDescriptor, LessThanOrEqualsBinaryOperator>()
				.Include<MaxDateTimeDescriptor, MaxDateTimeOperator>()
				.Include<MaxDescriptor, MaxOperator>()
				.Include<MemberInitDescriptor, MemberInitOperator>()
				.Include<MemberSelectorDescriptor, MemberSelectorOperator>()
				.Include<MinDateTimeDescriptor, MinDateTimeOperator>()
				.Include<MinDescriptor, MinOperator>()
				.Include<MinuteDescriptor, MinuteOperator>()
				.Include<ModuloBinaryDescriptor, ModuloBinaryOperator>()
				.Include<MonthDescriptor, MonthOperator>()
				.Include<MultiplyBinaryDescriptor, MultiplyBinaryOperator>()
				.Include<NegateDescriptor, NegateOperator>()
				.Include<NotEqualsBinaryDescriptor, NotEqualsBinaryOperator>()
				.Include<NotDescriptor, NotOperator>()
				.Include<NowDateTimeDescriptor, NowDateTimeOperator>()
				.Include<OrBinaryDescriptor, OrBinaryOperator>()
				.Include<OrderByDescriptor, OrderByOperator>()
				.Include<ParameterDescriptor, ParameterOperator>()
				.Include<RoundDescriptor, RoundOperator>()
				.Include<SecondDescriptor, SecondOperator>()
				.Include<SelectManyDescriptor, SelectManyOperator>()
				.Include<SelectDescriptor, SelectOperator>()
				.Include<SelectorLambdaDescriptor, SelectorLambdaOperator>()
				.Include<SingleDescriptor, SingleOperator>()
				.Include<SingleOrDefaultDescriptor, SingleOrDefaultOperator>()
				.Include<SkipDescriptor, SkipOperator>()
				.Include<StartsWithDescriptor, StartsWithOperator>()
				.Include<SubstringDescriptor, SubstringOperator>()
				.Include<SubtractBinaryDescriptor, SubtractBinaryOperator>()
				.Include<SumDescriptor, SumOperator>()
				.Include<TakeDescriptor, TakeOperator>()
				.Include<ThenByDescriptor, ThenByOperator>()
				.Include<TimeDescriptor, TimeOperator>()
				.Include<ToListDescriptor, ToListOperator>()
				.Include<ToLowerDescriptor, ToLowerOperator>()
				.Include<TotalOffsetMinutesDescriptor, TotalOffsetMinutesOperator>()
				.Include<TotalSecondsDescriptor, TotalSecondsOperator>()
				.Include<ToUpperDescriptor, ToUpperOperator>()
				.Include<TrimDescriptor, TrimOperator>()
                .Include<UnionDescriptor, UnionOperator>()
                .Include<WhereDescriptor, WhereOperator>()
				.Include<YearDescriptor, YearOperator>();
		}
	}
}
