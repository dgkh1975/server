using AutoFixture;
using AutoFixture.Dsl;
using Bit.Core.Models.Data;

namespace Bit.Core.Test.AutoFixture.CipherAttachmentMetaData
{
    public class MetaData : ICustomization
    {
        protected virtual IPostprocessComposer<CipherAttachment.MetaData> ComposerAction(IFixture fixture,
            ICustomizationComposer<CipherAttachment.MetaData> composer)
        {
            return composer.With(d => d.Size, fixture.Create<long>()).Without(d => d.SizeString);
        }
        public void Customize(IFixture fixture)
        {
            fixture.Customize<CipherAttachment.MetaData>(composer => ComposerAction(fixture, composer));
        }
    }

    public class MetaDataWithoutContainer : MetaData
    {
        protected override IPostprocessComposer<CipherAttachment.MetaData> ComposerAction(IFixture fixture,
            ICustomizationComposer<CipherAttachment.MetaData> composer) =>
            base.ComposerAction(fixture, composer).With(d => d.ContainerName, (string)null);
    }

    public class MetaDataWithoutKey : MetaDataWithoutContainer
    {
        protected override IPostprocessComposer<CipherAttachment.MetaData> ComposerAction(IFixture fixture,
            ICustomizationComposer<CipherAttachment.MetaData> composer) =>
            base.ComposerAction(fixture, composer).Without(d => d.Key);
    }
}
