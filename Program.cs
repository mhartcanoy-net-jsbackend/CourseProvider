using CourseProvider.Infrast.Data.Contexts;
using CourseProvider.Infrast.GraphQL;
using CourseProvider.Infrast.GraphQL.Mutations;
using CourseProvider.Infrast.GraphQL.ObjectTypes;
using CourseProvider.Infrast.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
	.ConfigureFunctionsWebApplication()
	.ConfigureServices(services =>
	{
		services.AddApplicationInsightsTelemetryWorkerService();
		services.ConfigureFunctionsApplicationInsights();
		services.AddPooledDbContextFactory<DataContext>(x => 
		{
			x.UseCosmos(Environment.GetEnvironmentVariable("COSMOS_URI")!, Environment.GetEnvironmentVariable("COSMOS_DB")!)
			.UseLazyLoadingProxies();

		});

		services.AddScoped<ICourseService, CourseService>();

		services.AddGraphQLFunction()
				.AddQueryType<Query>()
				.AddMutationType<CourseMutation>()
				.AddType<CourseType>();
		        

		var sp = services.BuildServiceProvider();
		using var scope = sp.CreateScope();
		var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<DataContext>>();
		using var context = dbContextFactory.CreateDbContext();
		context.Database.EnsureCreated();

		//* Hallå jag blir galen !! context.Database.EnsureCreated skjuter ut ett felmeddelade. Hittade felet 06-02 kl 14.27*//
	})
	.Build();

host.Run();
