﻿// Copyright (c) 2012, 2013 Semyon Grigorev <rsdpisuy@gmail.com>
// All rights reserved.
//
// The contents of this file are made available under the terms of the
// Eclipse Public License v1.0 (the "License") which accompanies this
// distribution, and is available at the following URL:
// http://www.opensource.org/licenses/eclipse-1.0.php
//
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either expressed or implied. See the License for
// the specific language governing rights and limitations under the License.
//
// By using this software in any fashion, you are agreeing to be bound by the
// terms of the License.

namespace Brahma.FSharp.OpenCL.Translator

open System.Collections.Generic
open Brahma.FSharp.OpenCL.AST
open System

exception InvalidKernelException of string

type ArrayKind =
    | CPointer
    | CArrayDecl of size: int

type Flags() =
    member val enableAtomic = false with get, set
    member val enableFP64 = false with get, set

type TranslatorOption =
    | UseNativeBooleanType
    | BoolAsBit

type TranslationContext<'lang, 'vDecl> =
    {
        TupleDecls: Dictionary<Type, StructType<'lang>>
        StructDecls: Dictionary<Type, StructType<'lang>>
        UnionDecls: Dictionary<Type, DiscriminatedUnionType<'lang>>
        UserDefinedTypes: ResizeArray<Type>

        TopLevelVarsDecls: ResizeArray<'vDecl>
        VarDecls: ResizeArray<'vDecl>

        ArrayKind: ArrayKind
        Namer: Namer
        Flags: Flags
        TranslatorOptions: TranslatorOption list
    }

    static member Create([<ParamArray>] translatorOptions: TranslatorOption[]) =
        {
            TupleDecls = Dictionary<Type, StructType<'lang>>()
            StructDecls = Dictionary<Type, StructType<'lang>>()
            UnionDecls = Dictionary<Type, DiscriminatedUnionType<'lang>>()
            UserDefinedTypes = ResizeArray<System.Type>()

            TopLevelVarsDecls = ResizeArray<'vDecl>()
            VarDecls = ResizeArray<'vDecl>()

            ArrayKind = CPointer
            Namer = Namer()
            Flags = Flags()
            TranslatorOptions = translatorOptions |> Array.toList
        }

    // TODO rewrite
    member this.DeepCopy() =
        let context = TranslationContext.Create(this.TranslatorOptions |> List.toArray)

        context.UserDefinedTypes.AddRange this.UserDefinedTypes

        for x in this.StructDecls do
            context.StructDecls.Add (x.Key,x.Value)
        for x in this.UnionDecls do
            context.UnionDecls.Add (x.Key,x.Value)
        for x in this.TupleDecls do
            context.TupleDecls.Add(x.Key,x.Value)

        context.Flags.enableFP64 <- this.Flags.enableFP64
        context.Flags.enableAtomic <- this.Flags.enableAtomic
        context

type TargetContext = TranslationContext<Lang, Statement<Lang>>

[<AutoOpen>]
module Translation =
    let translation = StateBuilder<TargetContext>()
