﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AutoStar.BuilderPattern
{
    static internal class BuilderExceptionGenerator
    {
        public static ClassDeclarationSyntax GenerateExceptionClass()
        {
            return ClassDeclaration("BuilderException")
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithBaseList(
                    BaseList(
                        SingletonSeparatedList<BaseTypeSyntax>(
                            SimpleBaseType(
                                QualifiedName(
                                    IdentifierName("System"),
                                    IdentifierName("Exception"))))))
                .WithMembers(
                    List(
                        new MemberDeclarationSyntax[]
                        {
                            ConstructorDeclaration(Identifier("BuilderException"))
                                .WithModifiers(
                                    TokenList(Token(SyntaxKind.PublicKeyword)))
                                .WithParameterList(
                                    ParameterList(
                                        SingletonSeparatedList(
                                            Parameter(
                                                    Identifier("validationFailures"))
                                                .WithType(
                                                    QualifiedName(
                                                        QualifiedName(
                                                            QualifiedName(
                                                                IdentifierName("System"),
                                                                IdentifierName(
                                                                    "Collections")),
                                                            IdentifierName("Generic")),
                                                        GenericName(Identifier("List"))
                                                            .WithTypeArgumentList(
                                                                TypeArgumentList(
                                                                    SingletonSeparatedList
                                                                        <TypeSyntax>(
                                                                            PredefinedType(
                                                                                Token(
                                                                                    SyntaxKind
                                                                                        .StringKeyword))))))))))
                                .WithInitializer(
                                    ConstructorInitializer(
                                        SyntaxKind.BaseConstructorInitializer,
                                        ArgumentList(
                                            SingletonSeparatedList(
                                                Argument(
                                                    InvocationExpression(
                                                            IdentifierName("GetMessage"))
                                                        .WithArgumentList(
                                                            ArgumentList(
                                                                SingletonSeparatedList(
                                                                    Argument(
                                                                        IdentifierName(
                                                                            "validationFailures"))))))))))
                                .WithBody(Block()),
                            MethodDeclaration(
                                    PredefinedType(Token(SyntaxKind.StringKeyword)),
                                    Identifier("GetMessage"))
                                .WithModifiers(
                                    TokenList(
                                        new[]
                                        {
                                            Token(SyntaxKind.PrivateKeyword),
                                            Token(SyntaxKind.StaticKeyword)
                                        }))
                                .WithParameterList(
                                    ParameterList(
                                        SingletonSeparatedList(
                                            Parameter(Identifier("validationFailures"))
                                                .WithType(
                                                    QualifiedName(
                                                        QualifiedName(
                                                            QualifiedName(
                                                                IdentifierName("System"),
                                                                IdentifierName(
                                                                    "Collections")),
                                                            IdentifierName("Generic")),
                                                        GenericName(
                                                                Identifier("IEnumerable"))
                                                            .WithTypeArgumentList(
                                                                TypeArgumentList(
                                                                    SingletonSeparatedList
                                                                    <
                                                                        TypeSyntax>(
                                                                        PredefinedType(
                                                                            Token(
                                                                                SyntaxKind
                                                                                    .StringKeyword))))))))))
                                .WithBody(
                                    Block(
                                        SingletonList<StatementSyntax>(
                                            ReturnStatement(
                                                InvocationExpression(
                                                        MemberAccessExpression(
                                                            SyntaxKind
                                                                .SimpleMemberAccessExpression,
                                                            PredefinedType(
                                                                Token(
                                                                    SyntaxKind
                                                                        .StringKeyword)),
                                                            IdentifierName("Join")))
                                                    .WithArgumentList(
                                                        ArgumentList(
                                                            SeparatedList<ArgumentSyntax>(
                                                                new SyntaxNodeOrToken[]
                                                                {
                                                                    Argument(
                                                                        LiteralExpression(
                                                                            SyntaxKind
                                                                                .StringLiteralExpression,
                                                                            Literal(
                                                                                "; "))),
                                                                    Token(
                                                                        SyntaxKind
                                                                            .CommaToken),
                                                                    Argument(
                                                                        IdentifierName(
                                                                            "validationFailures"))
                                                                })))))))
                        }));
        }
    }
}