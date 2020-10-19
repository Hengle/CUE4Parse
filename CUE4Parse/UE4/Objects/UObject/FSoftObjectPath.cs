﻿using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Exceptions;
using CUE4Parse.UE4.Versions;

namespace CUE4Parse.UE4.Objects.UObject
{
    public readonly struct FSoftObjectPath : IUStruct
    {
        /** Asset path, patch to a top level object in a package. This is /package/path.assetname */
        public readonly FName AssetPathName;
        /** Optional FString for subobject within an asset. This is the sub path after the : */
        public readonly string SubPathString;

        public readonly Package? Owner;

        public FSoftObjectPath(FAssetArchive Ar)
        {
            if (Ar.Ver < UE4Version.VER_UE4_ADDED_SOFT_OBJECT_PATH)
            {
                var path = Ar.ReadFString();
                throw new ParserException(Ar, $"Asset path \"{path}\" is in short form and is not supported, nor recommended");
            }
            else
            {
                AssetPathName = Ar.ReadFName();
                SubPathString = Ar.ReadFString();
            }

            Owner = Ar.Owner;
        }
        
        #region Loading Methods
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UExport Load() =>
            Load(Owner?.Provider ?? throw new ParserException("Package was loaded without a IFileProvider"));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryLoad(out UExport export)
        {
            var provider = Owner?.Provider;
            if (provider == null)
            {
                export = default;
                return false;
            }
            return TryLoad(provider, out export);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Load<T>() where T : UExport =>
            Load<T>(Owner?.Provider ?? throw new ParserException("Package was loaded without a IFileProvider"));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryLoad<T>(out T export) where T : UExport
        {
            var provider = Owner?.Provider;
            if (provider == null)
            {
                export = default;
                return false;
            }
            return TryLoad(provider, out export);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<UExport> LoadAsync() => await LoadAsync(Owner?.Provider ?? throw new ParserException("Package was loaded without a IFileProvider"));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<UExport?> TryLoadAsync()
        {
            var provider = Owner?.Provider;
            if (provider == null) return null;
            return await TryLoadAsync(provider).ConfigureAwait(false);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<T> LoadAsync<T>() where T : UExport => await LoadAsync<T>(Owner?.Provider ?? throw new ParserException("Package was loaded without a IFileProvider"));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<T?> TryLoadAsync<T>() where T : UExport
        {
            var provider = Owner?.Provider;
            if (provider == null) return null;
            return await TryLoadAsync<T>(provider).ConfigureAwait(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Load<T>(IFileProvider provider) where T : UExport =>
            Load(provider) as T ?? throw new ParserException("Loaded SoftObjectProperty but it was of wrong type");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryLoad<T>(IFileProvider provider, out T export) where T : UExport
        {
            if (!TryLoad(provider, out var genericExport) || !(genericExport is T cast))
            {
                export = default;
                return false;
            }

            export = cast;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<T> LoadAsync<T>(IFileProvider provider) where T : UExport => await LoadAsync(provider) as T ??
            throw new ParserException("Loaded SoftObjectProperty but it was of wrong type");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<T?> TryLoadAsync<T>(IFileProvider provider) where T : UExport => await TryLoadAsync(provider) as T;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UExport Load(IFileProvider provider) => provider.LoadObject(AssetPathName.Text);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryLoad(IFileProvider provider, out UExport export) =>
            provider.TryLoadObject(AssetPathName.Text, out export);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<UExport> LoadAsync(IFileProvider provider) => await provider.LoadObjectAsync(AssetPathName.Text);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<UExport?> TryLoadAsync(IFileProvider provider) =>
            await provider.TryLoadObjectAsync(AssetPathName.Text);
        
        #endregion

        public override string ToString() => string.IsNullOrEmpty(SubPathString)
            ? (AssetPathName.IsNone ? "" : AssetPathName.Text)
            : $"{AssetPathName.Text}:{SubPathString}";
    }
}
