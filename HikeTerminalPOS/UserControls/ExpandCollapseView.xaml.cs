using System.Windows.Input;

namespace HikePOS.UserControls;

public partial class ExpandCollapseView : Grid
{
    public ExpandCollapseView()
    {
        InitializeComponent();
        CollapseImage.Source = HikePOS.Resources.AppImages.ExpandIcon;
        UpdateEnabledState();
    }

    public View ContainerContent
    {
        get { return ContainerView.Content; }
        set { ContainerView.Content = value; }
    }


    public Color ContentBackgroundColor
    {
        get { return (Color)GetValue(ContentBackgroundColorProperty); }
        set { SetValue(ContentBackgroundColorProperty, value); }
    }

    public static readonly BindableProperty ContentBackgroundColorProperty =
        BindableProperty.Create(nameof(ContentBackgroundColor), typeof(Color), typeof(ExpandCollapseView), Colors.Transparent, BindingMode.TwoWay, propertyChanged: OnContentBackgroundColorChanged);


    static void OnContentBackgroundColorChanged(BindableObject bindable, object oldvalue, object newvalue)
    {
        var picker = bindable as ExpandCollapseView;
        picker.ContainerView.BackgroundColor = (Color)newvalue;
    }


    public string Title
    {
        get { return (string)GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title),typeof(string), typeof(ExpandCollapseView), default(string), BindingMode.TwoWay, propertyChanged: OnTitleChanged);


    static void OnTitleChanged(BindableObject bindable, object oldvalue, object newvalue)
    {
        var picker = bindable as ExpandCollapseView;
        picker.HeaderLabel.Text = newvalue?.ToString().ToUpper();
    }

    public bool IsActive
    {
        get { return (bool)GetValue(IsActiveProperty); }
        set
        {
            SetValue(IsActiveProperty, value);
        }
    }

    public static readonly BindableProperty IsActiveProperty =
        BindableProperty.Create("IsActive", typeof(bool), typeof(ExpandCollapseView), false, propertyChanged: OnIsActiveChanged);

    static void OnIsActiveChanged(BindableObject bindable, object oldValue, object newValue)
    {
        try
        {
            var picker = bindable as ExpandCollapseView;
            ////picker.IsActive = (bool)newValue;

            if ((bool)newValue == true)
            {
                //picker.VerticalOptions = LayoutOptions.FillAndExpand;
                //picker.ContainerView.VerticalOptions = LayoutOptions.FillAndExpand;
                picker.ContainerView.IsVisible = true;
                picker.ContainerViewTopLine.IsVisible = true;
                picker.ContainerViewTopLine.Margin = new Thickness(picker.Padding.Left * -1,0,picker.Padding.Right * -1,0);
                picker.CollapseImage.RotateTo(180, 1);
            }
            else
            {
                //picker.VerticalOptions = LayoutOptions.Start;
                //picker.ContainerView.VerticalOptions = LayoutOptions.Start;
                picker.ContainerView.IsVisible = false;
                picker.ContainerViewTopLine.IsVisible = false;
                picker.CollapseImage.RotateTo(0, 1);
            }
        }
        catch (Exception ex)
        {
            ex.Track();
        }

    }


    public ICommand Command
    {
        get { return (ICommand)GetValue(CommandProperty); }
        set { SetValue(CommandProperty, value); }
    }

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command),typeof(ICommand), typeof(ExpandCollapseView),default(Command),BindingMode.TwoWay,null, propertyChanged: OnCommandChanged);

    private static void OnCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        try
        {
            var picker = bindable as ExpandCollapseView;
            picker.CollapseButton.Command = (ICommand)newValue;

            if (oldValue != null)
                ((ICommand)oldValue).CanExecuteChanged -= picker.OnCommandCanExecuteChanged;
            if (newValue != null)
                ((ICommand)newValue).CanExecuteChanged += picker.OnCommandCanExecuteChanged;

            picker.UpdateEnabledState();
        }
        catch (Exception ex)
        {
            ex.Track();
        }

    }

    public object CommandParameter
    {
        get { return CollapseButton.CommandParameter; }
        set { CollapseButton.CommandParameter = value; }
    }


    void UpdateEnabledState()
    {
        IsEnabled = Command?.CanExecute(CommandParameter) ?? false;
    }



    void OnCommandCanExecuteChanged(object sender, EventArgs e)
    {
        IsEnabled = Command?.CanExecute(CommandParameter) ?? false;
    }



}

