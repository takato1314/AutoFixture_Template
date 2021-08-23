# AutoFixture_Template
An [AutoFixture](https://github.com/AutoFixture/AutoFixture) setup template that create mocks with pre-determined values.
- Mocks are based on loose mocks based on AutoMoqCustomization.
- Provides an [IFixtureSetup](https://github.com/takato1314/autofixture_extensions/blob/main/src/Core/Model/IFixtureSetup.cs) extensibility point that allows users to setup mock fixtures based on fixed values if intended so.
  -  Implementations of IFixtureSetup will use recursive mocks by default to provide values.
  -  Implementations of IFixtureSetup will freeze it's instance (as singleton) onto the fixture. This is done so that any other dependent fixture objects will refer to the same instance.
- All public properties and methods can be setup via Mock.Setup method.
- Since ["AutoFixture is an opinionated library, and one of the opinions it holds is that nulls are invalid return values."](https://stackoverflow.com/questions/21921789/why-does-autofixture-automoq-make-recursive-mocks-by-default#comment33213527_21921789), [nullable types are not handled in AutoFixture](https://github.com/AutoFixture/AutoFixture/issues/731). This AutoFixture extension intends to provide random values to that Nullable type by referring to its base type by default; else you can also pass in `null` value if [desired so](https://github.com/takato1314/autofixture_extensions/blob/main/tests/Core.Tests/Model/ComplexChildFixture.cs#L32).

## TO DO
- Add options to allow users to determine the object lifetime (currently Singleton) of the IFixtureSetup object. The same goes to the [Inject()](https://github.com/takato1314/autofixture_extensions/blob/main/src/Core/Model/BaseFixtureSetup.cs#L43) method.
- Add options to [turn off recursive mocks](https://stackoverflow.com/questions/21921789/why-does-autofixture-automoq-make-recursive-mocks-by-default#comment33213527_21921789).
