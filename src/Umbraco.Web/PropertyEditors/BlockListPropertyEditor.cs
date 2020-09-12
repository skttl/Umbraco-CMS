using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Blocks;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a block list property editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.BlockList,
        "Block List",
        "blocklist",
        ValueType = ValueTypes.Json,
        Group = Constants.PropertyEditors.Groups.Lists,
        Icon = "icon-thumbnail-list")]
    public class BlockListPropertyEditor : BlockEditorPropertyEditor
    {
        private readonly ILocalizedTextService _localizedTextService;
        private readonly Lazy<PropertyEditorCollection> _propertyEditors;
        private readonly IDataTypeService _dataTypeService;
        private readonly IContentTypeService _contentTypeService;

        public BlockListPropertyEditor(ILogger logger, Lazy<PropertyEditorCollection> propertyEditors, IDataTypeService dataTypeService, IContentTypeService contentTypeService, ILocalizedTextService localizedTextService)
            : base(logger, propertyEditors, dataTypeService, contentTypeService, localizedTextService)
        {
            _localizedTextService = localizedTextService;
            _propertyEditors = propertyEditors;
            _dataTypeService = dataTypeService;
            _contentTypeService = contentTypeService;
        }

        // has to be lazy else circular dep in ctor
        private PropertyEditorCollection PropertyEditors => _propertyEditors.Value;

        #region Pre Value Editor

        protected override IConfigurationEditor CreateConfigurationEditor() => new BlockListConfigurationEditor();

        #endregion

        protected override IDataValueEditor CreateValueEditor() => new BlockListEditorPropertyValueEditor(Attribute, PropertyEditors, _dataTypeService, _contentTypeService, _localizedTextService, Logger);

        internal class BlockListEditorPropertyValueEditor : BlockEditorPropertyValueEditor
        {
            private readonly ILocalizedTextService _localizedTextService;

            public BlockListEditorPropertyValueEditor(DataEditorAttribute attribute, PropertyEditorCollection propertyEditors, IDataTypeService dataTypeService, IContentTypeService contentTypeService, ILocalizedTextService textService, ILogger logger)
                : base(attribute, propertyEditors, dataTypeService, contentTypeService, textService, logger)
            {
                _localizedTextService = textService;
            }

            /// <inheritdoc />
            public override object Configuration
            {
                get => base.Configuration;
                set
                {
                    if (value == null)
                        throw new ArgumentNullException(nameof(value));
                    if (!(value is BlockListConfiguration configuration))
                        throw new ArgumentException($"Expected a {typeof(BlockListConfiguration).Name} instance, but got {value.GetType().Name}.", nameof(value));

                    if (configuration.HelpText.IsNullOrWhiteSpace())
                    {
                        configuration.HelpText = _localizedTextService.Localize("grid/addElement");
                    }
                    else
                    {
                        configuration.HelpText = _localizedTextService.UmbracoDictionaryTranslate(configuration.HelpText);
                    }

                    base.Configuration = configuration;

                    HideLabel = configuration.HideLabel.TryConvertTo<bool>().Result;
                }
            }
        }
    }
}
