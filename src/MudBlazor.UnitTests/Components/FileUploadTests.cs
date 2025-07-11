﻿// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudBlazor.UnitTests.Dummy;
using MudBlazor.UnitTests.Mocks;
using MudBlazor.UnitTests.TestComponents.FileUpload;
using NUnit.Framework;

namespace MudBlazor.UnitTests.Components
{
    [TestFixture]
    public class FileUploadTests : BunitTest
    {
        /// <summary>
        /// Verifies that invalid T values are logged using the provided ILogger
        /// </summary>
        [Test]
        public void InvalidTLogWarning_Test()
        {
            var provider = new MockLoggerProvider();
            var logger = provider.CreateLogger(GetType().FullName) as MockLogger;
            Context.Services.AddLogging(x => x.ClearProviders().AddProvider(provider)); //set up the logging provider
            var comp = Context.RenderComponent<MudFileUpload<MudTextField<string>>>();

            var entries = logger.GetEntries();
            entries.Count.Should().Be(1);
            entries[0].Level.Should().Be(LogLevel.Warning);
            entries[0].Message.Should().Be(string.Format("T must be of type {0} or {1}",
                typeof(IReadOnlyList<IBrowserFile>), typeof(IBrowserFile)));
        }

        /// <summary>
        /// Checks the FileUpload CSS classes
        /// </summary>
        [Test]
        public void FileUpload_CSSTest()
        {
            var comp = Context.RenderComponent<MudFileUpload<IBrowserFile>>(parameters => parameters
                .Add(x => x.Class, "outer-test")
                .Add(x => x.InputClass, "inner-test"));

            comp.Find(".mud-input-control.mud-file-upload.outer-test"); //find outer div

            var innerClasses = comp.Find("input").GetAttribute("class"); //find inner input
            innerClasses.Should().Be("inner-test");
        }

        /// <summary>
        /// Ensures the underlying input receives the multiple attribute
        /// </summary>
        [Test]
        public void FileUpload_MultipleTest()
        {
            var comp = Context.RenderComponent<MudFileUpload<IReadOnlyList<IBrowserFile>>>();

            var input = comp.Find("input");
            input.HasAttribute("multiple").Should().BeTrue();
        }

        /// <summary>
        /// Ensures the underlying input receives the hidden attribute (default case)
        /// </summary>
        [Test]
        public void FileUpload_HiddenTest1()
        {
            var comp = Context.RenderComponent<MudFileUpload<IReadOnlyList<IBrowserFile>>>();

            var input = comp.Find("input");
            input.HasAttribute("hidden").Should().BeTrue();
        }

        /// <summary>
        /// Ensures the underlying input does not receive the hidden attribute
        /// </summary>
        [Test]
        public void FileUpload_HiddenTest2()
        {
            var comp = Context.RenderComponent<MudFileUpload<IReadOnlyList<IBrowserFile>>>(parameters =>
                parameters.Add(x => x.Hidden, false));

            var input = comp.Find("input");
            input.HasAttribute("hidden").Should().BeFalse();
        }

        /// <summary>
        /// Ensures the underyling input receives the accept attribute
        /// </summary>
        [Test]
        public void FileUpload_AcceptTest()
        {
            var comp = Context.RenderComponent<MudFileUpload<IBrowserFile>>(parameters => parameters
                .Add(x => x.Accept, ".png, .jpg"));

            var input = comp.Find("input");
            input.GetAttribute("accept").Should().Be(".png, .jpg");
        }

        /// <summary>
        /// Verifies the button template renders
        /// </summary>
        [Test]
        public void FileUpload_ButtonTemplateContextTest_Renders()
        {
            var comp = Context.RenderComponent<FileUploadWithDragAndDropActivatorTest>();

            var openFilePickerButton = comp.Find("button#open-file-picker-button");
            openFilePickerButton.ToMarkup().Should().Contain("Open file picker");

            var clearButton = comp.Find("button#clear-button");
            clearButton.ToMarkup().Should().Contain("Clear");
        }

        /// <summary>
        /// Verifies the ClearAsync function clears the Files property
        /// </summary>
        [Test]
        public async Task FileUpload_ClearAsync_Should_Clear_Files()
        {
            var fileName = "cat.jpg";
            var defaultFile = new DummyBrowserFile(fileName, DateTimeOffset.Now, 0, "image/jpeg", []);
            var comp = Context.RenderComponent<FileUploadWithDragAndDropActivatorTest>(
                ComponentParameterFactory.Parameter(nameof(FileUploadWithDragAndDropActivatorTest.File), defaultFile));
            var fileUploadComp = comp.FindComponent<MudFileUpload<IBrowserFile>>();
            var fileUploadInstance = fileUploadComp.Instance;

            fileUploadInstance.Files.Should().NotBeNull();
            fileUploadInstance.Files!.Name.Should().Be(fileName);

            await comp.InvokeAsync(() => comp.Find("button#clear-button").Click());

            fileUploadInstance.Files.Should().BeNull();
        }

        /// <summary>
        /// Verifies the OpenFilePickerAsync method opens the file picker when the file picker button is clicked
        /// <remarks>
        /// Native HTML buttons trigger the onclick event when the space or enter keys are pressed.
        /// If users use something that does not render a native button, they will need to add the appropriate keyboard event handlers.
        /// </remarks>
        /// </summary>
        [Test]
        public async Task FileUpload_OpenFilePickerAsync_Should_OpenFilePicker_When_Clicked()
        {
            var comp = Context.RenderComponent<FileUploadWithDragAndDropActivatorTest>();

            await comp.InvokeAsync(() => comp.Find("button#open-file-picker-button").Click());

            Context.JSInterop.Invocations.Should().ContainSingle(invocation => invocation.Identifier == "mudFileUpload.openFilePicker");
        }

        /// <summary>
        /// Tests the OnFilesChangedEvent
        /// </summary>
        [Test]
        public async Task FileUpload_OnFilesChangedTest()
        {
            var fileContent = InputFileContent.CreateFromText("Garderoben is a farmer!", "upload.txt");

            var comp = Context.RenderComponent<FileUploadOnFilesChangedTest>();

            var input = comp.FindComponent<InputFile>();
            input.UploadFiles(fileContent);

            comp.Instance.File.Name.Should().Be("upload.txt");
            var fileString = await comp.Instance.File.GetFileContents();

            fileString.Should().Be("Garderoben is a farmer!");
        }

        /// <summary>
        /// Tests the FileValueChanged event bound to a form
        /// </summary>
        [Test]
        public async Task FileUpload_FileValueChangedTest()
        {
            InputFileContent[] fileContent =
            {
                InputFileContent.CreateFromText("Garderoben is a farmer!", "upload.txt"),
                InputFileContent.CreateFromText("A Balrog, servant of Morgoth", "upload2.txt")
            };

            var comp = Context.RenderComponent<FileUploadFormValidationTest>();

            var inputs = comp.FindComponents<InputFile>();
            inputs.Count.Should().Be(2);

            inputs[0].UploadFiles(fileContent[0]); //upload single file

            comp.Instance.Model.File.Should().NotBeNull();
            comp.Instance.Model.File.Name.Should().Be("upload.txt");
            var fileString = await comp.Instance.Model.File.GetFileContents();
            fileString.Should().Be("Garderoben is a farmer!");

            inputs[1].UploadFiles(fileContent); //upload both files

            comp.Instance.Model.Files.Count.Should().Be(2);
            comp.Instance.Model.Files[0].Name.Should().Be("upload.txt");
            comp.Instance.Model.Files[1].Name.Should().Be("upload2.txt");
            var fileString1 = await comp.Instance.Model.Files[0].GetFileContents();
            fileString1.Should().Be("Garderoben is a farmer!");
            var fileString2 = await comp.Instance.Model.Files[1].GetFileContents();
            fileString2.Should().Be("A Balrog, servant of Morgoth");
        }

        /// <summary>
        /// Tests the FileValueChanged event bound to a form with validation
        /// </summary>
        [Test]
        public async Task FileUpload_ValidationTest()
        {
            InputFileContent[] fileContent =
            {
                InputFileContent.CreateFromText("Garderoben is a farmer!", "upload.txt"),
                InputFileContent.CreateFromText("A Balrog, servant of Morgoth", "upload2.txt")
            };

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; //<<< rework this!
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            var comp = Context.RenderComponent<FileUploadFormValidationTest>();

            var form = comp.Instance.Form;
            await comp.InvokeAsync(() => form.Validate());

            form.IsValid.Should().BeFalse(); //form is invalid to start

            var single = comp.FindComponent<MudFileUpload<IBrowserFile>>();
            single.Instance.ErrorText.Should().Be("'File' must not be empty.");
            single.Markup.Should().Contain("'File' must not be empty.");

            var multiple = comp.FindComponent<MudFileUpload<IReadOnlyList<IBrowserFile>>>();
            multiple.Instance.ErrorText.Should().Be("'Files' must not be empty.");
            multiple.Markup.Should().Contain("'Files' must not be empty.");

            var singleInput = single.FindComponent<InputFile>();
            singleInput.UploadFiles(fileContent[0]); //upload first file

            await comp.InvokeAsync(() => form.Validate());

            single.Instance.ErrorText.Should().Be(null); //first input is now valid
            single.Markup.Should().NotContain("'File' must not be empty.");

            form.IsValid.Should().BeFalse(); //form is still invalid

            var multipleInput = multiple.FindComponent<InputFile>();
            multipleInput.UploadFiles(fileContent); //upload second files

            await comp.InvokeAsync(() => form.Validate());

            single.Instance.ErrorText.Should().Be(null); //second input is now valid
            single.Markup.Should().NotContain("'Files' must not be empty.");

            form.IsValid.Should().BeTrue(); //form is now valid
        }

        /// <summary>
        /// Tests that more than 10 files can be uploaded
        /// </summary>
        [Test]
        public void FileUpload_MaximumFileCountTest()
        {
            List<InputFileContent> Files = new();
            for (var i = 0; i < 11; i++)
            {
                Files.Add(InputFileContent.CreateFromText("Garderoben is a farmer!", $"upload{i}.txt"));
            }

            Files.Count.Should().Be(11); //ensure there are 11 files

            var comp = Context.RenderComponent<FileUploadMultipleFilesTest>();

            var multiple = comp.FindComponent<MudFileUpload<IReadOnlyList<IBrowserFile>>>();
            var multipleInput = multiple.FindComponent<InputFile>();
            multipleInput.UploadFiles(Files.ToArray()); //upload second files

            comp.Instance.Files.Count.Should()
                .Be(11); //if no error occurs, we have successfully uploaded more than 10 files
        }

        /// <summary>
        /// Makes sure the file upload is disabled
        /// </summary>
        [Test]
        public void FileUploadDisabledTest()
        {
            var comp = Context.RenderComponent<FileUploadDisabledTest>();
            comp.FindComponent<MudFileUpload<IBrowserFile>>().Find("input").HasAttribute("disabled").Should().BeFalse();
            comp.FindComponent<MudFileUpload<IBrowserFile>>().Find("button").HasAttribute("disabled").Should().BeFalse();


            comp.SetParametersAndRender(parameters =>
                parameters.Add(x => x.Disabled,
                    true)); //The input and child button should be disabled when file upload is disabled

            comp.FindComponent<MudFileUpload<IBrowserFile>>().Find("input").HasAttribute("disabled").Should().BeTrue();
            comp.FindComponent<MudFileUpload<IBrowserFile>>().Find("button").HasAttribute("disabled").Should()
                .BeTrue(); //we need to test for a button as the MudButton replaces disabled labels with buttons
        }

        /// <summary>
        /// Verifies files are appended correctly
        /// </summary>
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void FileUploadAppendMultipleTest(bool appendMultiple)
        {
            var comp = Context.RenderComponent<FileUploadAppendMultipleTest>(p =>
                p.Add(x => x.AppendMultipleFiles, appendMultiple));

            var input = comp.FindComponent<InputFile>();
            input.UploadFiles(GenerateFile(), GenerateFile(), GenerateFile()); //upload first file
            comp.Instance.Files.Count.Should().Be(3);

            input.UploadFiles(GenerateFile());
            comp.Instance.Files.Count.Should().Be(appendMultiple ? 4 : 1);

            InputFileContent GenerateFile()
            {
                return InputFileContent.CreateFromText("snakex64 is Canadian", $"{Guid.NewGuid()}.txt");
            }
        }

        /// <summary>
        /// Optional FileUpload should not have required attribute and aria-required should be false.
        /// </summary>
        [Test]
        public void OptionalFileUpload_Should_NotHaveRequiredAttributeAndAriaRequiredShouldBeFalse()
        {
            var comp = Context.RenderComponent<MudFileUpload<IBrowserFile>>();

            comp.Find("input").HasAttribute("required").Should().BeFalse();
            comp.Find("input").GetAttribute("aria-required").Should().Be("false");
        }

        /// <summary>
        /// Required FileUpload should have required and aria-required attributes.
        /// </summary>
        [Test]
        public void RequiredFileUpload_Should_HaveRequiredAndAriaRequiredAttributes()
        {
            var comp = Context.RenderComponent<MudFileUpload<IBrowserFile>>(parameters => parameters
                .Add(p => p.Required, true));

            comp.Find("input").HasAttribute("required").Should().BeTrue();
            comp.Find("input").GetAttribute("aria-required").Should().Be("true");
        }

        /// <summary>
        /// Required and aria-required FileUpload attributes should be dynamic.
        /// </summary>
        [Test]
        public void RequiredAndAriaRequiredFileUploadAttributes_Should_BeDynamic()
        {
            var comp = Context.RenderComponent<MudFileUpload<IBrowserFile>>();

            comp.Find("input").HasAttribute("required").Should().BeFalse();
            comp.Find("input").GetAttribute("aria-required").Should().Be("false");

            comp.SetParametersAndRender(parameters => parameters
                .Add(p => p.Required, true));

            comp.Find("input").HasAttribute("required").Should().BeTrue();
            comp.Find("input").GetAttribute("aria-required").Should().Be("true");
        }

        /// <summary>
        /// FileUpload should generate new InputFile on file change.
        /// </summary>
        [Test]
        public async Task Generate_new_InputFile_on_file_change()
        {
            var comp = Context.RenderComponent<MudFileUpload<IBrowserFile>>();

            // only 1 input element should be present
            comp.FindAll("input").Should().HaveCount(1);

            // trigger an OnChange on the internal InputFile
            var defaultFile = new DummyBrowserFile("filename.jpg", DateTimeOffset.Now, 0, "image/jpeg", []);
            await comp.InvokeAsync(() => comp.FindComponent<InputFile>().Instance.OnChange.InvokeAsync(new InputFileChangeEventArgs([defaultFile])));

            // 2 input elements should now be present
            // one should be visible
            comp.FindAll("input:not(.d-none)").Should().HaveCount(1);
            // and the other should no longer be visible
            comp.FindAll("input.d-none").Should().HaveCount(1);
        }

        /// <summary>
        /// FileUpload should trigger the FilesChanged and OnFilesChanged callbacks when appropriate.
        /// </summary>
        [Test]
        public async Task Should_trigger_file_change_callbacks_as_expected()
        {
            var comp = Context.RenderComponent<FileUploadChangeCountTests>();

            // first file change should trigger both callbacks
            var fileContent = new byte[5];
            // fill file content with random bytes
            new Random().NextBytes(fileContent);
            var firstFile = new DummyBrowserFile("filename.jpg", DateTimeOffset.Now, 0, "image/jpeg", fileContent);
            await comp.InvokeAsync(() => comp.FindComponents<InputFile>()[0].Instance.OnChange.InvokeAsync(new InputFileChangeEventArgs([firstFile])));

            comp.Instance.FilesChangedCount.Should().Be(1);
            comp.Instance.OnFilesChangedCount.Should().Be(1);

            // since a new InputFile is generated with each upload, we can get the last InputFile in the render chain to emulate a new upload
            // so when a new file reference is uploaded, both file change callbacks should be triggered
            new Random().NextBytes(fileContent);
            var secondFile = new DummyBrowserFile("filename.jpg", DateTimeOffset.Now, 0, "image/jpeg", fileContent);
            await comp.InvokeAsync(() => comp.FindComponents<InputFile>()[^1].Instance.OnChange.InvokeAsync(new InputFileChangeEventArgs([secondFile])));

            comp.Instance.FilesChangedCount.Should().Be(2);
            comp.Instance.OnFilesChangedCount.Should().Be(2);
        }

        private static InputFileContent CreateDummyFile(string fileName, long size)
        {
            var content = new byte[size];
            var file = new DummyBrowserFile(fileName, DateTimeOffset.Now, size, "application/octet-stream", content);

            return InputFileContent.CreateFromBinary(file.Content, file.Name, null, file.ContentType);
        }

        [Test]
        public void MaxFileSize_SingleFile_WithinLimit()
        {
            var comp = Context.RenderComponent<FileUploadSingleFileTest>(parameters => parameters.Add(p => p.MaxFileSize, 100L));

            var file = CreateDummyFile("test.txt", 50);
            var input = comp.FindComponent<InputFile>();

            input.UploadFiles(file);

            comp.Instance.File.Should().NotBeNull();
            comp.Instance.File.Name.Should().Be("test.txt");
            comp.Instance.File.Size.Should().Be(50);
        }

        [Test]
        public void MaxFileSize_SingleFile_ExceedsLimit()
        {
            var comp = Context.RenderComponent<FileUploadSingleFileTest>(parameters => parameters.Add(p => p.MaxFileSize, 100L));

            var file = CreateDummyFile("test.txt", 150);
            var input = comp.FindComponent<InputFile>();
            var fileUpload = comp.FindComponent<MudFileUpload<IBrowserFile>>().Instance;

            input.UploadFiles(file);

            comp.Instance.File.Should().BeNull(); // File should be rejected
            fileUpload.Error.Should().BeTrue();
            fileUpload.ErrorText.Should().Be("File 'test.txt' exceeds the maximum allowed size of 100 bytes.");
        }

        [Test]
        public void MaxFileSize_SingleFile_NoLimit()
        {
            var comp = Context.RenderComponent<FileUploadSingleFileTest>(parameters => parameters.Add(p => p.MaxFileSize, null));

            var file = CreateDummyFile("test.txt", 200);
            var input = comp.FindComponent<InputFile>();
            var fileUpload = comp.FindComponent<MudFileUpload<IBrowserFile>>().Instance;

            input.UploadFiles(file);

            comp.Instance.File.Should().NotBeNull();
            comp.Instance.File.Name.Should().Be("test.txt");
            comp.Instance.File.Size.Should().Be(200);
            fileUpload.Error.Should().BeFalse();
            fileUpload.ErrorText.Should().BeNullOrEmpty();
        }

        [Test]
        public void MaxFileSize_MultipleFiles_AllWithinLimit()
        {
            var comp = Context.RenderComponent<FileUploadMultipleFilesTest>(parameters => parameters.Add(p => p.MaxFileSize, 100L));

            var file1 = CreateDummyFile("test1.txt", 50);
            var file2 = CreateDummyFile("test2.txt", 70);

            var input = comp.FindComponent<InputFile>();
            var fileUpload = comp.FindComponent<MudFileUpload<IReadOnlyList<IBrowserFile>>>().Instance;

            input.UploadFiles(file1, file2);

            comp.Instance.Files.Should().NotBeNull();
            comp.Instance.Files.Count.Should().Be(2);
            comp.Instance.Files[0].Name.Should().Be("test1.txt");
            comp.Instance.Files[1].Name.Should().Be("test2.txt");
            fileUpload.Error.Should().BeFalse();
            fileUpload.ErrorText.Should().BeNullOrEmpty();
        }

        [Test]
        public void MaxFileSize_MultipleFiles_SomeExceedLimit()
        {
            var comp = Context.RenderComponent<FileUploadMultipleFilesTest>(parameters => parameters.Add(p => p.MaxFileSize, 100L));

            var file1 = CreateDummyFile("test1.txt", 50);
            var file2 = CreateDummyFile("test2.txt", 120);
            var file3 = CreateDummyFile("test3.txt", 70);

            var input = comp.FindComponent<InputFile>();
            var fileUpload = comp.FindComponent<MudFileUpload<IReadOnlyList<IBrowserFile>>>().Instance;

            input.UploadFiles(file1, file2, file3);

            // Assertions after OnChangeAsync
            comp.Instance.Files.Should().NotBeNull();
            comp.Instance.Files.Count.Should().Be(2);
            comp.Instance.Files.Should().Contain(f => f.Name == "test1.txt");
            comp.Instance.Files.Should().Contain(f => f.Name == "test3.txt");
            fileUpload.Error.Should().BeTrue();
            fileUpload.ErrorText.Should().Be("File 'test2.txt' exceeds the maximum allowed size of 100 bytes.");
        }

        [Test]
        public void MaxFileSize_MultipleFiles_AllExceedLimit()
        {
            var comp = Context.RenderComponent<FileUploadMultipleFilesTest>(parameters => parameters.Add(p => p.MaxFileSize, 100L));

            var file1 = CreateDummyFile("test1.txt", 120);
            var file2 = CreateDummyFile("test2.txt", 150);

            var input = comp.FindComponent<InputFile>();

            input.UploadFiles(file1, file2);
            var fileUpload = comp.FindComponent<MudFileUpload<IReadOnlyList<IBrowserFile>>>().Instance;

            comp.Instance.Files.Should().NotBeNull(); // It will be an empty list
            comp.Instance.Files.Count.Should().Be(0);
            fileUpload.Error.Should().BeTrue();

            var validationErrors = fileUpload.ValidationErrors;
            validationErrors.Should().HaveCount(2);
            validationErrors.Should().Contain("File 'test1.txt' exceeds the maximum allowed size of 100 bytes.");
            validationErrors.Should().Contain("File 'test2.txt' exceeds the maximum allowed size of 100 bytes.");
        }

        [Test]
        public void MaxFileSize_MultipleFiles_NoLimit()
        {
            var comp = Context.RenderComponent<FileUploadMultipleFilesTest>(parameters => parameters.Add(p => p.MaxFileSize, null));

            var file1 = CreateDummyFile("test1.txt", 200);
            var file2 = CreateDummyFile("test2.txt", 300);

            var input = comp.FindComponent<InputFile>();
            var fileUpload = comp.FindComponent<MudFileUpload<IReadOnlyList<IBrowserFile>>>().Instance;

            input.UploadFiles(file1, file2);

            comp.Instance.Files.Should().NotBeNull();
            comp.Instance.Files.Count.Should().Be(2);
            comp.Instance.Files[0].Name.Should().Be("test1.txt");
            comp.Instance.Files[1].Name.Should().Be("test2.txt");
            fileUpload.Error.Should().BeFalse();
            fileUpload.ErrorText.Should().BeNullOrEmpty();
        }

        [Test]
        public async Task MaxFileSize_ClearValidationAfterError()
        {
            var comp = Context.RenderComponent<FileUploadMultipleFilesTest>(parameters => parameters.Add(p => p.MaxFileSize, 100));

            var file1 = CreateDummyFile("test1.txt", 200);
            var file2 = CreateDummyFile("test2.txt", 300);

            var input = comp.FindComponent<InputFile>();

            input.UploadFiles(file1, file2);

            // Assert initial error state
            comp.Instance.Files.Should().BeEmpty();

            var fileUpload = comp.FindComponent<MudFileUpload<IReadOnlyList<IBrowserFile>>>().Instance;

            fileUpload.Error.Should().BeTrue();
            fileUpload.ErrorText.Should().Be("File 'test1.txt' exceeds the maximum allowed size of 100 bytes.");

            await comp.InvokeAsync(fileUpload.ClearAsync);

            // Assert cleared state
            comp.Instance.Files.Should().BeNull();
            fileUpload.Error.Should().BeFalse(); // Errors should be cleared
            fileUpload.ErrorText.Should().BeNullOrEmpty(); // ErrorText should be cleared
            fileUpload.ValidationErrors.Should().BeEmpty(); // ValidationErrors related to MaxFileSize should be cleared
        }

        [Test]
        public async Task MaxFileSize_ResetValidationAfterError()
        {
            var comp = Context.RenderComponent<FileUploadSingleFileTest>(parameters => parameters.Add(p => p.MaxFileSize, 100));

            var file1 = CreateDummyFile("test1.txt", 200);

            var input = comp.FindComponent<InputFile>();
            var fileUpload = comp.FindComponent<MudFileUpload<IBrowserFile>>().Instance;

            input.UploadFiles(file1);

            // Assert initial error state
            comp.Instance.File.Should().BeNull();
            fileUpload.Error.Should().BeTrue();
            fileUpload.ErrorText.Should().Be("File 'test1.txt' exceeds the maximum allowed size of 100 bytes.");

            await comp.InvokeAsync(fileUpload.ResetValidation);

            // Assert cleared state
            comp.Instance.File.Should().BeNull();
            fileUpload.Error.Should().BeFalse(); // Errors should be cleared
            fileUpload.ErrorText.Should().BeNullOrEmpty(); // ErrorText should be cleared
            fileUpload.ValidationErrors.Should().BeEmpty(); // ValidationErrors related to MaxFileSize should be cleared
        }
    }
}
