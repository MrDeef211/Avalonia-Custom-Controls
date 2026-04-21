# Подключение

В `App.axaml` подключите стили контролов:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceInclude Source="avares://Controls/Themes/Controls.axaml" />
            ...
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
  
    <Application.Styles>
        <StyleInclude Source="avares://Controls/Themes/Controls.Styles.axaml" />
        ...
    </Application.Styles>
</Application>
```

Для библиотеки нужны AvaloniaUI и ReactiveUI

---
# Обзор контролов

| Контрол |	Описание |
| :--- | :--- |
| `DataFormControl` | 	Форма редактирования объекта с автоматической генерацией полей, категориями и валидацией. |
| `PropertyGridControl` | 	Редактор свойств объектов. Автоматически отображает все публичные свойства, группирует по категориям, поддерживает редактирование на лету. |
| `ChartControl` | 	Линейный график с различными стилями отрисовки, сеткой, масштабированием и подсказками. |
| `PieChart` | 	Круговая диаграмма с возможностью выделения секторов, настройкой цветов и меток. |
| `RangeSlider` | 	Ползунок выбора диапазона значений с двумя бегунками и текстовыми полями. |
| `ZoomControl` | 	Контейнер с поддержкой масштабирования и панорамирования содержимого. |
| `Toolbar` | 	Панель инструментов с единообразным стилем кнопок и разделителей. |
| `ToolbarGroup` | 	Группа элементов в тулбаре с подписью снизу. |

# Кастомизация стилей

Все контролы поддерживают два уровня настройки:

- **Через StyledProperty** – цвета, шрифты, размеры.

- **Через стилевые классы** – каждый важный элемент имеет CSS-подобный класс (например, `DataFormSaveButton`). Это позволяет переопределить внешний вид глобально без изменения шаблонов.

Пример глобального стиля в `App.axaml`:

```xml
<Style Selector="Button.DataFormSaveButton">
    <Setter Property="Background" Value="#2E7D32" />
</Style>
```
---
# DataFormControl
Автоматически строит форму редактирования для любого объекта. Поддерживает группировку по категориям, валидацию и команды.

## Базовое использование

```xml
<controls:DataFormControl x:Name="MyForm"
                          SelectedObject="{Binding MyObject}"
                          FormConfig="{Binding MyConfig}"
                          HasChanges="{Binding HasChanges}"
                          SaveCommand="{Binding SaveCommand}"
                          ButtonPanelPlacement="InsideBottom" />
```

## Конфигурация формы
Конфигурация задаётся через класс `DataFormConfig` либо через атрибуты. Её можно не указывать – тогда форма будет построена только на основе атрибутов модели. Параметры `DataFormConfig` имеют приоритет и переопределяют соответствующие атрибуты.

**Через атрибуты в модели:**

```csharp
public class Person
{
    [Category("Основные")]
    [DisplayName("Полное имя")]
    [Required]
    public string FullName { get; set; }

    [Range(1, 120)]
    public int Age { get; set; }

    [Browsable(false)]
    public string InternalId { get; set; }
}
```
**Через `DataFormConfig` (программно):**

```csharp
var config = new DataFormConfig();
config.SetFieldRule("FullName", new DataFormFieldConfig
{
    DisplayName = "ФИО",
    Category = "Личные данные",
    Order = 1,
    Validation = new DataFormFieldValidation { MaxLength = 100 }
});

config.SetCategoryOrder("Личные данные", 1);
config.SetCategoryCollapsible("Личные данные", true, false); // развёрнута

MyForm.FormConfig = config;
```
### Свойства конфигурационной модели `DataFormConfig`:

| Свойство (readonly)	| Описание	|
| :--- | :--- |
| `CategoryCollapsible` | Словарь ***категория - значение***, определяет, могут ли категории сворачиваться пользователем. |
| `CategoryExpanded` | Словарь ***категория - значение***, начальное состояние развёрнутости категорий. |
| `CategoryOrders` | Словарь ***категория - значение***, порядок отображения категорий. |
| `Fields` | Словарь ***поле - значение (`DataFormFieldConfig`)***, конфигурации отдельных полей. |

### Методы конфигурационной модели:

| Метод	| Описание |
| :--- | :--- |
| `AddFieldRule` | Добавляет конфигурации отдельных полей по имени свойства. Добавляет новые правила к старым. |
| `AddFieldValidation` | Добавляет правила валидации отдельных полей по имени свойства. Добавляет новые правила к старым. |
| `SetFieldRule` | Устанавливает конфигурации отдельных полей по имени свойства. Заменяет старое правило новым. |
| `SetFieldValidation` | Устанавливает правила валидации отдельных полей по имени свойства. Заменяет старое правило новым. |
| `SetCategoryOrder` | Устанавливает порядок отображения категории. |
| `SetCategoryCollapsible` | Устанавливает возможность схлопывания категории. Имеет перегрузку с настройкой начальной свёрнутости категории. |

### Основные свойства `DataFormFieldConfig`:

| Свойство	| Тип	| По умолчанию	| Описание	|
| :--- | :--- | :--- | :--- |
| `DisplayName`	| `string?` | `null` | Подпись поля.	|
| `Category`	| `string?` | `null` | Категория (секция).	|
| `IsReadOnly`	| `bool?` | `null` | Только для чтения.	|
| `IsRequired`	| `bool?` | `null` | Обязательное поле.	|
| `IsBrowsable`	| `bool` | `true` | Показывать ли поле.	|
| `Order`	| `int` | `0` | Порядок в категории.	|
| `HideLabel`	| `bool` | `false` | Скрыть метку.	|
| `RowGroup`	| `int` | `-1` | Объединить поля в одну строку (поля с одинаковым RowGroup располагаются горизонтально).	|
| `Validation`	| `DataFormFieldValidation?` | `null` | Правила валидации (см. ниже).	|

### Валидация
Поддерживаются два уровня:

- **Атрибуты DataAnnotations** – `[Required]`, `[Range]`, `[StringLength]`, `[RegularExpression]`.

- **Программная конфигурация** через `DataFormFieldValidation`.

```csharp
config.SetFieldValidation("Email", new DataFormFieldValidation
{
    RegexPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
    CustomErrorMessage = "Некорректный email"
});
```
Если поле не помечено `[Required]`, пустое значение считается валидным (даже при наличии `RegexPattern`).

`DataFormFieldValidation` переопределяет настройки от атрибутов.

**Основные свойства `DataFormFieldValidation`**
| Свойство	| Тип | Описание	|
| :--- | :--- | :--- |
| `Min` | `double?` | Минимальное допустимое значение (для числовых типов). | 
| `Max` | `double?` | Максимальное допустимое значение (для числовых типов). | 
| `MaxLength` | `int?` | Максимальная длина строки. | 
| `RegexPattern` | `string?` | Регулярное выражение для проверки строки. | 
| `CustomErrorMessage` | `string?` | Пользовательское сообщение об ошибке. | 

## Настройка через свойства контрола

`DataFormControl` потдерживает настройку через `StyledProperty` при установке в **axaml** файле.

### Основная настройка

**Изменяемый обьект и его настройка** устанавливается через свойства:

- `SelectedObject` – Обьект, источник данных

- `FormConfig` – Конфигурационный обьект формы (`DataFormConfig`)

- `IsReadOnly` – Устанавливает возможность менять данные формы

### Команды

**Для привязки команд к событиям сохранения и отмены** `DataFormControl` потдерживает свойста комманд для привязки

- `SaveCommand` – Команда, выполняемая после успешного сохранения данных. Параметр, сохранённый объект.

- `CancelCommand` – Команда, выполняемая после отмены изменений. Параметр, изменяемый обьект.

### Макет

**Расположение встроенных кнопок** задаётся через свойство `ButtonPanelPlacement`:

- `None` – не показывать (По умолчанию).

- `Top` / `Bottom` – снаружи скроллируемой области.

- `InsideTop` / `InsideBottom` – внутри области прокрутки.

Привязанные к кнопкам команды доступны и могут быть привязаны к другим кнопкам / другой логике:

- `CommitChangesCommand` – применяет изменения.

- `CancelChangesCommand` – откатывает изменения.

Свойство `HasChanges` позволяет отслеживать наличие несохранённых изменений.

**Длина подписей** задаётся через свойство `LabelColumnWidtht` и имеет тип `GridLength`:

- `Auto` – Автоматически по длине подписи.
- `Число` – Конкретное значение (по умолчанию, `120`).
- `Число*` – Пропорционально, относительно длины полей ввода.

**Отступы полей** задаётся через свойство `FieldMargin`, по умолчанию `new Thickness(4)`

### Свойства стилизации

| Свойство	| Тип	| По умолчанию	| Описание |
| :--- | :--- | :--- | :--- |
| `LabelFontSize` | `double` | `12.0` | Размер шрифта колонки имён свойств. |
| `LabelFontWeight` | `FontWeight` | `FontWeight.SemiBold)` | Тип шрифта колонки имён свойств. |
| `LabelForeground` | `IBrush?` | `Brushes.Black` | Цвет шрифта колонки имён свойств |
| `CategoryHeaderFontSize` | `double` | `14.0` | Размер шрифта заголовков |
| `CategoryHeaderFontWeight` | `FontWeight` | `FontWeight.Bold` | Тип шрифта заголовков |
| `CategoryHeaderForeground` | `IBrush?` | `Brushes.Black` | Цвет шрифта заголовков |

## Стилизация DataFormControl

Потдерживается подробная настройка стилей элементов через стилевые классы.

**Доступные стилевые классы:**

| Класс	| элемент	|
| :--- | :--- |
| `DataFormSaveButton` | Кнопка «Сохранить» |
| `DataFormCancelButton` | Кнопка «Отмена» |
| `DataFormSectionHeader` | Заголовок секции |
| `DataFormFieldLabel` | Метка поля |
| `DataFormEditor` | Редактор поля (`TextBox`, `NumericUpDown` и т.д.) |
| `DataFormErrorIcon` | Иконка ошибки (`TextBlock`) |

Пример:

```xml
<Style Selector="TextBlock.DataFormFieldLabel">
    <Setter Property="FontWeight" Value="SemiBold" />
</Style>
```

## Поведение в приложении

- **Редактирование** – при изменении значения в поле оно немедленно применяется к исходному объекту. Свойство HasChanges (унаследовано от `BaseEditorControl`) показывает, были ли внесены изменения.

- **Сохранение изменений** – вызов `CommitChanges()` записывает значение в обьект и буфер, фиксируя значения для последующей работы, в конце вызывает выполнение `SaveCommand`.

- **Отмена изменений** – вызов `CancelChanges()` перезагружает все значения из буфера, откатывая несохранённые правки, в конце вызывает выполнение `CancelCommand`.

- **Типы редакторов** – контрол автоматически подбирает подходящий редактор в зависимости от типа свойства:

  * `bool` → `CheckBox`

  * `enum` → `ComboBox`

  * Числовые типы → `NumericUpDown`

  * `DateTime` / `DateTime?` → `DatePicker`

  * Остальные → `TextBox`

- **ReadOnly свойства** – свойства без публичного сеттера или с атрибутом `[ReadOnly]` отображаются, но недоступны для изменения.

---

# PropertyGridControl

`PropertyGridControl` — это редактор свойств объектов в стиле `PropertyGrid`. Он автоматически отображает все публичные свойства переданного объекта, группирует их по категориям и предоставляет удобный интерфейс для редактирования значений на лету.

## Базовое использование

```xml
<controls:PropertyGridControl x:Name="PropertyGrid"
                              SelectedObject="{Binding MyObject}"
                              IsReadOnly="False" />
```

При изменении `SelectedObject` грид автоматически перестраивается.

## Конфигурация через атрибуты

`PropertyGridControl` в первую очередь опирается на атрибуты модели (при наличии). Никакой дополнительной конфигурации не требуется.

**Поддерживаемые атрибуты:**

| Атрибут	| Применение | 
| :--- | :--- |
| `[Category("...")]`	| Группировка свойств в раскрывающиеся секции. | 
| `[DisplayName("...")]`	| Отображаемое имя свойства (по умолчанию – имя свойства). | 
| `[Description("...")]`	| Всплывающая подсказка при наведении на имя свойства. | 
| `[Browsable(false)]`	| Полностью скрывает свойство из грида. | 
| `[ReadOnly(true)]`	| Отключает редактирование поля (также учитывается IsReadOnly контрола и наличие private set). | 
| Атрибуты валидации	| `[Required]`, `[Range]`, `[StringLength]`, `[RegularExpression]` – см. раздел Валидация. | 

Пример модели:

```csharp
public class Person
{
    [Category("Основные")]
    [DisplayName("Имя")]
    [Required]
    public string Name { get; set; }

    [Category("Основные")]
    [Range(1, 120)]
    public int Age { get; set; }

    [Description("Внутренний идентификатор")]
    [ReadOnly(true)]
    public Guid Id { get; set; }

    [Browsable(false)]
    public string Secret { get; set; }
}
```

### Валидация

`PropertyGridControl` использует атрибуты `DataAnnotations` для проверки вводимых значений.

- Если свойство помечено `[Required]`, пустое значение (или null) вызовет ошибку.

- Если поле не обязательно, пустая строка или null считаются допустимыми, даже при наличии `[StringLength]` или `[RegularExpression]`.

- При возникновении ошибки рядом с полем появляется иконка ⚠️, а всплывающая подсказка показывает текст ошибки.

## Настройка через свойства контрола

`PropertyGridControl` поддерживает настройку визуальных параметров через `StyledProperty` в **axaml.**

### Основные свойства

| Свойство	| Тип	| По умолчанию	| Описание | 
| :--- | :--- | :--- | :--- |
| `SelectedObject`	| `object?`	| `null`	| `Объект, свойства которого отображаются.` | 
| `IsReadOnly`	| `bool`	| `false`	| `Глобальный запрет редактирования всех полей.` | 

### Макет

| Свойство	| Тип	| По умолчанию	| Описание | 
| :--- | :--- | :--- | :--- |
| `PropertyNameColumnWidth`	| `GridLength`	| `Auto`	| Ширина колонки с именами свойств. Можно задать `Auto` фиксированное число или пропорцию (`2*`). | 
| `EditorMargin`	| `Thickness`	| `4`	| Отступы вокруг редактора значения. | 

### Свойства стилизации

| Свойство	| Тип	| По умолчанию	| Описание | 
| :--- | :--- | :--- | :--- |
| `PropertyNameFontSize`	| `double`	| `12`	| Размер шрифта имени свойства. | 
| `PropertyNameFontWeight`	| `FontWeight`	| `SemiBold`	| Жирность шрифта имени свойства. | 
| `PropertyNameForeground`	| `IBrush?`	| `Black`	| Цвет текста имени свойства. | 
| `ErrorIconForeground`	| `IBrush?`	| `Red`	| Цвет иконки ошибки. | 
| `CategoryHeaderFontSize`	| `double`	| `14`	| Размер шрифта заголовка категории. | 
| `CategoryHeaderFontWeight`	| `FontWeight`	| `Bold`	| Жирность шрифта заголовка категории. | 

## Стилизация через классы

`PropertyGridControl` предоставляет стилевые классы для тонкой настройки внешнего вида без изменения шаблона.

| Класс	| Элемент	| Назначение | 
| :--- | :--- | :--- |
| `PropertyGridExpander`	| `Expander`	| Заголовок категории (секции). | 
| `PropertyGridFieldLabel`	| `TextBlock`	| Имя свойства. | 
| `PropertyGridEditor`	| `Разные`	| Элементы ввода: `TextBox`, `CheckBox`, `ComboBox`, `NumericUpDown`, `DatePicker`. | 
| `PropertyGridErrorIcon` | `TextBlock` | Иконка ошибки валидации. | 

Пример глобальной стилизации в `App.axaml`:

```xml
<Style Selector="TextBlock.PropertyGridFieldLabel">
    <Setter Property="FontStyle" Value="Italic" />
</Style>

<Style Selector="Expander.PropertyGridExpander">
    <Setter Property="Foreground" Value="#2C3E50" />
</Style>

<Style Selector="TextBox.PropertyGridEditor, NumericUpDown.PropertyGridEditor, ComboBox.PropertyGridEditor, DatePicker.PropertyGridEditor">
    <Setter Property="BorderThickness" Value="1" />
    <Setter Property="BorderBrush" Value="#CCCCCC" />
</Style>
```

## Поведение в приложении

- **Редактирование** – при изменении значения в поле оно немедленно применяется к исходному объекту. Свойство HasChanges (унаследовано от `BaseEditorControl`) показывает, были ли внесены изменения.

- **Подсказки** – при наведении на имя свойства отображается его описание (`[Description]`). Для enum полей во всплывающей подсказке также перечисляются все возможные значения с их описаниями.

- **Типы редакторов** – контрол автоматически подбирает подходящий редактор в зависимости от типа свойства:

  * `bool` → `CheckBox`

  * `enum` → `ComboBox`

  * Числовые типы → `NumericUpDown`

  * `DateTime` / `DateTime?` → `DatePicker`

  * Остальные → `TextBox`

- **ReadOnly свойства** – свойства без публичного сеттера или с атрибутом `[ReadOnly]` отображаются, но недоступны для изменения.

---

# ChartControl

`ChartControl` – это полностью настраиваемый линейный график, отрисовываемый через `DrawingContext`. Поддерживает несколько стилей линий, сетку, оси, подписи точек и интерактивное масштабирование/панорамирование.

## Базовое использование

Данные передаются через свойство `Content`, которое принимает объект `ChartDataBase` (словарь `double` → `double`). Контрол автоматически сортирует точки по ключу (`X`).

```xml
<controls:ChartControl x:Name="MyChart"
                       ChartStyle="Line"
                       ChartColor="DodgerBlue"
                       Fill="False"
                       Interactive="True" />
```

```csharp
var data = new ChartDataBase();
data.Add(1, 10.5);
data.Add(2, 25.3);
data.Add(3, 18.7);
MyChart.Content = data;
```

## Данные и оси

### Модель данных ChartDataBase

| Метод	| Описание | 
| :--- | :--- |
| `Add(double key, double value)`	| Добавляет точку по координатам. | 
| `Add(double value)`	| Добавляет точку с автоматическим ключом (макс. ключ + 1). | 
| `Remove(double key)`	| Удаляет точку. | 
| `Count`	| Количество точек. | 

Контрол автоматически определяет границы по `X` (`Minimum` / `Maximum`) при установке `Content`. По `Y` масштабируется локально в пределах видимого диапазона.

### Режимы подписей осей (`AxisLabelMode`) 

Задаётся отдельно для `X` (`LabelModeX`) и `Y` (`LabelModeY`):

- `None` – без подписей.

- `Grid` – по линиям сетки (использует настройки `GridSizeX` / `GridSizeY`).

- `Points` – только в точках данных (полезно для категориальных данных).

- `Auto` – автоматический выбор красивого шага.

## Визуальные настройки

### Стиль графика и заливка

| Свойство	| Тип	| По умолчанию	| Описание | 
| :--- | :--- | :--- | :--- |
| `ChartStyle`	| `ChartStyle`	| `Simple`	| Стиль соединения точек: `Simple` (только точки/вертикальные линии), `Line` (ломаная), `Step` (ступенчатый), `Spline` (сглаженный). | 
| `ChartColor`	| `IBrush`	| `DodgerBlue`	| Цвет линии и точек. | 
| `ChartThickness`	| `double`	| `2`	| Толщина линии. | 
| `Fill`	| `bool`	| `false`	| Заливка области под графиком цветом `ChartColor`. | 
| `HighlightPoints`	| `bool`	| `true`	| Отображать ли точки на линии. | 
| `TiltThreshold` | `double` | `24` | Порог наклона текста в пикселях |

### Сетка и оси

| Свойство	| Тип	| По умолчанию	| Описание | 
| :--- | :--- | :--- | :--- |
| `Grid`	| `bool`	| `true`	| Показывать координатную сетку. | 
| `GridColor`	| `IBrush`	| `LightGray`	| Цвет линий сетки. | 
| `GridSizeX`	| `GridLength`	| `Auto`	| Шаг сетки по `X`. `Auto` – автоматический подбор; число – фиксированный шаг; `N*` – примерно `N` линий. | 
| `GridSizeY`	| `GridLength`	| `Auto`	| Шаг сетки по `Y`. | 
| `Axis`	| `bool`	| `true`	| Показывать оси координат (`X` и `Y`). | 
| `AxisColor`	| `IBrush`	| `Green`	| Цвет осей и засечек. | 

### Подписи точек

| Свойство	| Тип	| По умолчанию	| Описание | 
| :--- | :--- | :--- | :--- |
| `ShowPointsLabels`	| `bool`	| `false`	| Отображать числовые значения над точками. | 
| `PointsLabelsColor`	| `IBrush`	| `Black`	| Цвет текста подписей. | 

### Шрифты

| Свойство	| Тип	| По умолчанию	| Описание | 
| :--- | :--- | :--- | :--- |
| `AxisLabelFontSize`	| `double`	| `10`	| Размер шрифта подписей осей. | 
| `AxisLabelFontFamily`	| `FontFamily`	| `Default`	| Шрифт подписей осей. | 
| `AxisLabelFontWeight`	| `FontWeight`	| `Normal`	| Жирность шрифта осей. | 
| `AxisLabelFontStyle`	| `FontStyle`	| `Normal`	| Стиль шрифта осей. | 
| `PointLabelFontSize`	| `double`	| `10`	| Размер шрифта подписей точек. | 
| `TooltipFontSize`	| `double`	| `12`	| Размер шрифта всплывающей подсказки. | 

### Интерактивность
Если `Interactive="True"`, пользователь может взаимодействовать с графиком:

- **Перетаскивание левой кнопкой** – панорамирование по оси `X`.

- **Колёсико мыши** – масштабирование по `X` относительно положения курсора.

- **Клавиши-стрелки (←/→)** – сдвиг графика на 5% диапазона.

- **Клавиши (↑/↓)** – увеличение / уменьшение масштаба на 20%.

- **Наведение мыши** – показывает всплывающую подсказку с координатами ближайшей точки.

Интерактивность можно активировать только при зажатой клавише-модификаторе, задав свойство `InteractiveModifier` (например, `Control`). Если модификатор не требуется, установите `InteractiveModifier="None"`. Наведение мыши настраивается отдельно.

## Форматирование чисел

Числа на осях и в подсказках автоматически форматируются:

- ≥ 1 млн → `1.2M`

- ≥ 1 тыс → `1.5k`

- Дробные → до двух знаков после запятой

- Целые → без дробной части

## Стилизация

Контрол не использует стилевые классы, так как полностью отрисовывается в коде. Настройка внешнего вида осуществляется исключительно через перечисленные выше `StyledProperty`. 
Границы и фон задаются через стандартные свойства `BorderBrush`, `BorderThickness`, `Background`.

```xml
<controls:ChartControl Background="#F9F9F9"
                       BorderBrush="Silver"
                       BorderThickness="1"
                       Padding="50,30,30,50" />
```

---

# PieChart

`PieChart` — круговая (или кольцевая) диаграмма, отрисовываемая полностью в коде. Поддерживает настройку цветов секторов, интерактивное выделение при наведении, гибкое размещение подписей, а также вставку изображения в центр диаграммы.

## Базовое использование

Данные передаются через свойство `Content` — объект `PieChartDataBase` (словарь `string` → `double`). Ключи используются как названия секторов, значения определяют их размер.

```xml
<controls:PieChart x:Name="MyPieChart"
                   ShowLabels="True"
                   ShowPercentages="True"
                   InnerRadius="0.3"
                   HighlightSector="True" />
```

```csharp
var data = new PieChartDataBase();
data.Add("Продукты", 450);
data.Add("Транспорт", 200);
data.Add("Развлечения", 150);
MyPieChart.Content = data;
```

### Модель данных PieChartDataBase

| Элемент	| Описание | 
| :--- | :--- |
| `Chart`	| Словарь `string` → `double`, доступный для чтения и записи. Изменение словаря вызывает уведомления. | 
| `Sum()`	| Сумма всех значений (используется для расчёта процентов). | 
| `Count`	| Количество секторов. | 
| `Add(key, value)`	| Добавляет новый сектор. | 
| `AddRange(items)`	| Добавляет несколько секторов за раз. | 
| `Remove(key)`	| Удаляет сектор. | 

**Важно:** `PieChartDataBase` реализует `ReactiveObject`, поэтому изменения автоматически обновляют диаграмму.

## Визуальные настройки

### Цвета секторов
Цвета можно задать через `SectorColors` (список кистей). Если цветов меньше, чем секторов, используются встроенные цвета по умолчанию (`DodgerBlue`, `OrangeRed`, `Gold`, `MediumSeaGreen`, `MediumPurple`, `HotPink`, `Teal`, `Coral`).

```csharp
MyPieChart.SectorColors = new List<IBrush>
{
    Brushes.Tomato,
    Brushes.SteelBlue,
    Brushes.YellowGreen
};
```

### Геометрия

| Свойство	| Тип	| По умолчанию	| Описание | 
| :--- | :--- | :--- | :--- |
| `InnerRadius`	| `double (0..1)`	| `0`	| Радиус внутреннего отверстия. **0** — обычная круговая диаграмма, **1** — кольцо с полностью открытым центром. | 
| `StartAngle`	| `double (0..360)`	| `0`	| Начальный угол первого сектора (в градусах). Отсчёт идёт от направления вверх (12 часов). | 
| `PaintHole`	| `bool`	| `false`	| Закрашивать ли центральную область цветом `HoleColor`. | 
| `HoleColor`	| `IBrush`	| `White`	| Цвет заливки центра при `PaintHole = true`. | 

### Подписи секторов

| Свойство	| Тип	| По умолчанию	| Описание | 
| :--- | :--- | :--- | :--- |
| `ShowLabels`	| `bool`	| `true`	| Показывать подписи секторов. | 
| `ShowPercentages`	| `bool`	| `true`	| Добавлять процентное значение к подписи (например, Продукты (56.3%)). | 
| `LabelPlacement`	| `LabelPlacement`	| `Outside`	| Расположение подписи: `Inside` (внутри сектора), `Outside` (снаружи), `Callout` (выноска с линией). | 
| `LabelFontSize`	| `double`	| `12`	| Размер шрифта подписей. | 
| `LabelFontFamily`	| `FontFamily`	| `Default`	| Шрифт подписей. | 
| `LabelFontWeight`	| `FontWeight`	| `Normal`	| Жирность шрифта. | 
| `LabelFontStyle`	| `FontStyle`	| `Normal`	| Стиль шрифта. | 

### Интерактивное выделение

При наведении мыши на сектор он может подсвечиваться.

| Свойство	| Тип	| По умолчанию	| Описание | 
| :--- | :--- | :--- | :--- |
| `HighlightSector`	| `bool`	| `true`	| Включить выделение сектора под курсором. | 
| `HighlightType`	| `HighlightType`	| `Push`	| Эффект выделения (см. таблицу ниже). | 
| `SizeShift`	| `double`	| `0.05`	| Смещение размера при выделении в относительных единицах. | 

**Эффекты выделения (`HighlightType`)**

| Значение | Описание | 
| :--- | :--- |
| `Push`	| Сектор немного выдвигается из центра без изменения формы. | 
| `Increase`	| Увеличивается внешний радиус, без смещения. | 
| `IncreaseOut`	| Комбинация выдвижения и увеличения радиуса. | 
| `Decrease`	| Внешний радиус уменьшается. | 
| `Reduce`	|  Пропорциональное уменьшение размера сектора (имитация «нажатия»). | 

### Центральное изображение

В центр диаграммы можно поместить изображение (например, логотип). Актуально при `InnerRadius > 0`.


| Свойство	| Тип	| По умолчанию	| Описание | 
| :--- | :--- | :--- | :--- |
| `CenterImage`	| `IImage?`	| `null`	| Изображение для отображения в центре. | 
| `ImageZoom`	| `double`	| `100`	| Коэффициент масштабирования изображения (в процентах от диаметра отверстия). | 
| `ImageScaling`	| `ImageScaling`	| `Fill`	| Режим масштабирования: `Fill` (растянуть), `Contain` (вписать полностью), `Cover` (заполнить с обрезкой). | 

## Стилизация

Контрол не использует стилевые классы — все визуальные параметры задаются через `StyledProperty`. Для изменения фона и границ используйте стандартные свойства:

```xml
<controls:PieChart Background="#FAFAFA"
                   BorderBrush="Silver"
                   BorderThickness="1" />
```

## Поведение в приложении

- При наведении курсора сектор подсвечивается согласно `HighlightType`.

- Диаграмма автоматически перерисовывается при изменении `Content`, цветов или геометрических параметров.

- Подписи автоматически подстраиваются под размер сектора и размещение (`LabelPlacement`).

---

# ZoomControl

`ZoomControl` — это контейнер, предоставляющий интерактивное масштабирование и панорамирование (зум и перетаскивание) для любого дочернего содержимого. 

## Базовое использование

Поместите любой элемент внутрь `ZoomControl`. По умолчанию содержимое отображается в исходном размере (масштаб 1.0).

```xml
<controls:ZoomControl x:Name="ZoomViewer"
                      MinScale="0.1"
                      MaxScale="10"
                      ZoomSpeed="1.2"
                      RestrictPan="True">
    <Image Source="large_image.jpg" />
</controls:ZoomControl>
```

## Основные возможности

- Масштабирование колёсиком мыши в точку курсора.

- Панорамирование перетаскиванием левой кнопкой мыши (или правой, если `EnablePanWithRightButton="True"`).

- Сброс масштаба двойным кликом или командой `ResetZoomCommand`.

- Вписывание в экран (`FitToScreenCommand`) — автоматически подбирает масштаб и центрирует содержимое.

- История действий — поддерживает отмену (**Undo**) и повтор (**Redo**) последних изменений (до 50 шагов).

- Клавиатурное управление — полностью настраиваемые горячие клавиши.

- Плавная анимация — при использовании клавиш перемещение происходит с интерполяцией.

## Свойства масштабирования и поведения

| Свойство	| Тип	| По умолчанию	| Описание | 
| :--- | :--- | :--- | :--- |
| `MinScale`	| `double`	| `0.1`	| Минимально допустимый масштаб (максимальное удаление). | 
| `MaxScale`	| `double`	| `10.0`	| Максимально допустимый масштаб (максимальное приближение). | 
| `ZoomSpeed`	| `double`	| `1.2`	| Коэффициент изменения масштаба за один шаг колеса мыши. | 
| `Smoothness`	| `double` (0.1..0.5)	| `0.5`	| Плавность анимации перемещения. Меньше — более плавно. | 
| `EnablePanWithRightButton`	| `bool`	| `true`	| Разрешить панорамирование правой кнопкой мыши. | 
| `RestrictPan`	| `bool`	| `true`	| Ограничивать панорамирование так, чтобы содержимое не уходило за края при минимальном масштабе. | 
| `EnableKeyboardNavigation`	| `bool`	| `true`	| Включить управление с клавиатуры. | 

## Управление состоянием через ZoomState

Вы можете привязаться к свойству `ZoomState` (тип `ZoomState`), чтобы сохранять и восстанавливать положение камеры.

```csharp
public class ZoomState : ReactiveObject
{
    public double Scale { get; set; }   // текущий масштаб
    public double OffsetX { get; set; } // смещение по X
    public double OffsetY { get; set; } // смещение по Y
}
```

```xml
<controls:ZoomControl ZoomState="{Binding MyZoomState}" />
```

Изменение `ZoomState` программно мгновенно перемещает камеру.

## Команды

Контрол предоставляет готовые команды, которые можно привязать к кнопкам интерфейса:

| Команда	| Описание | 
| :--- | :--- |
| `ResetZoomCommand`	| Сбрасывает масштаб до `1.0` и смещение в `(0,0)`. | 
| `FitToScreenCommand`	| Подгоняет содержимое под размер контрола с сохранением пропорций. | 
| `UndoCommand`	| Отменяет последнее действие (шаг назад). | 
| `RedoCommand`	| Повторяет отменённое действие (шаг вперёд). | 

Пример привязки:

```xml
<Button Command="{Binding ElementName=ZoomViewer, Path=FitToScreenCommand}">Вписать</Button>
```

## Настройка клавиш

Все горячие клавиши можно переопределить через свойства. Доступны как основные клавиши, так и альтернативные (например, стрелки).

### Панорамирование

| Свойство	| По умолчанию	| Альтернатива (по умолчанию) | 
| :--- | :--- | :--- |
| `PanUpKey`	| `W`	| `PanUpAltKey = Up` | 
| `PanDownKey`	| `S`	| `PanDownAltKey = Down` | 
| `PanLeftKey`	| `A`	| `PanLeftAltKey = Left` | 
| `PanRightKey`	| `D`	| `PanRightAltKey = Right` | 

Чтобы отключить альтернативные клавиши, установите их в `None`.

### Масштабирование

| Свойство	| По умолчанию | 
| :--- | :--- |
| `ZoomInKey`	| `Add` (и всегда `OemPlus`) | 
| `ZoomOutKey`	| `Subtract` (и всегда `OemMinus`) | 

### Сброс и вписывание

| Свойство	| Клавиша по умолчанию	| Модификатор | 
| :--- | :--- | :--- |
| `ResetZoomKey` / `ResetZoomModifiers`	| `D0` | `Control` | 
| `FitToScreenKey` / `FitToScreenModifiers`	| `Home`	| `None` | 

### История

| Свойство	| Клавиша по умолчанию	| Модификатор | 
| :--- | :--- | :--- |
| `UndoKey` / `UndoModifiers`	| `Z`	| `Control` | 
| `RedoKey` / `RedoModifiers`	| `Y`	| `Control` | 

Пример переназначения:

```xml
<controls:ZoomControl PanUpKey="Up"
                      PanDownKey="Down"
                      PanLeftKey="Left"
                      PanRightKey="Right"
                      PanUpAltKey="None"
                      ResetZoomKey="R"
                      ResetZoomModifiers="Control" />
```

## Стилизация
`ZoomControl` наследует `ContentControl` и использует стандартные свойства `Background`, `BorderBrush`, `BorderThickness`. Внутренний элемент `PART_Presenter` (`Border`) применяет `RenderTransform` для масштабирования и смещения. Для дополнительной кастомизации можно переопределить шаблон, но обычно это не требуется.

```xml
<controls:ZoomControl Background="Black"
                      BorderBrush="Gray"
                      BorderThickness="1" />
```

## Поведение в приложении
- **Перетаскивание:** зажмите левую кнопку мыши и перемещайте курсор. Если `EnablePanWithRightButton="True"`, можно использовать правую кнопку.

- **Масштабирование:** крутите колёсико мыши вверх/вниз. Масштабирование происходит относительно положения курсора.

- **Клавиатура:** при фокусе на контроле используйте назначенные клавиши для перемещения, масштабирования и навигации по истории.

- **Двойной клик:** сбрасывает масштаб к исходному (`ResetZoom`).

---

# RangeSlider

`RangeSlider` — это слайдер с двумя независимыми ползунками, предназначенный для выбора диапазона значений. Он поддерживает текстовые поля ввода, настраиваемую заливку трека, засечки и плавное перетаскивание. Идеален для фильтров, настроек временных интервалов или любых задач, где требуется указать нижнюю и верхнюю границы.

## Базовое использование

```xml
<controls:RangeSlider x:Name="MySlider"
                      Minimum="0"
                      Maximum="100"
                      LowerValue="20"
                      UpperValue="80"
                      Step="1"
                      ShowTextBoxes="True"
                      FillMode="Between"
                      FillBrush="DodgerBlue" />
```

Значения `LowerValue` и `UpperValue` поддерживают двустороннюю привязку, поэтому их можно легко синхронизировать с **ViewModel**.

```csharp
public class ViewModel : ReactiveObject
{
    private double _minPrice = 100;
    private double _maxPrice = 500;
    public double MinPrice { get => _minPrice; set => this.RaiseAndSetIfChanged(ref _minPrice, value); }
    public double MaxPrice { get => _maxPrice; set => this.RaiseAndSetIfChanged(ref _maxPrice, value); }
}
```
```xml
<controls:RangeSlider LowerValue="{Binding MinPrice}"
                      UpperValue="{Binding MaxPrice}"
                      Minimum="0" Maximum="1000" />
```
## Настройка через свойства
### Основные свойства диапазона и поведения
| Свойство	| Тип	| По умолчанию	| Описание | 
| :--- | :--- | :--- | :--- |
| `Minimum`	| `double`	| `0`	| Минимально возможное значение на шкале. | 
| `Maximum`	| `double`	| `100`	| Максимально возможное значение на шкале. | 
| `LowerValue`	| `double`	| `0`	| Текущее значение нижнего (левого) ползунка. | 
| `UpperValue`	| `double`	| `100`	| Текущее значение верхнего (правого) ползунка. | 
| `Step`	| `double`	| `1`	| Шаг изменения значений. При перетаскивании ползунки «прилипают» к величинам, кратным `Step`. | 
| `IsLowerThumbEnabled`	| `bool`	| `true`	| Разрешить перетаскивание нижнего ползунка. При `false` ползунок и связанное текстовое поле блокируются. | 
| `IsUpperThumbEnabled`	| `bool`	| `true`	| Разрешить перетаскивание верхнего ползунка. | 

### Текстовые поля
Текстовые поля позволяют вводить точные числовые значения. Они отображаются слева (для `LowerValue`) и справа (для `UpperValue`).

| Свойство	| Тип	| По умолчанию	| Описание | 
| :--- | :--- | :--- | :--- |
| `ShowTextBoxes`	| `bool`	| `true`	| Показывать ли текстовые поля ввода. | 
| `TextBoxWidth`	| `double`	| `50`	| Ширина каждого текстового поля. | 

При вводе значения автоматически проверяется, что оно находится в допустимых пределах (с учётом `Minimum`, `Maximum`, `Step` и позиции соседнего ползунка). Ввод пустой строки сбрасывает значение до `Minimum`.

### Визуальные настройки трека и заливки
| Свойство	| Тип	| По умолчанию	| Описание | 
| :--- | :--- | :--- | :--- |
| `TrackBrush`	| `IBrush`	| `LightGray`	| Цвет линии трека. | 
| `LowerThumbBrush`	| `IBrush`	| `Gray`	| Цвет нижнего ползунка. | 
| `UpperThumbBrush`	| `IBrush`	| `Gray`	| Цвет верхнего ползунка. |  
| `FillMode`	| `FillMode`	| `Between`	| Режим заливки трека (см. таблицу ниже). | 
| `FillBrush`	| `IBrush`	| `DodgerBlue`	| Цвет заливки трека. | 

### Режимы заливки (FillMode)
| Значение	| Описание | 
| :--- | :--- |
| `None`	| Заливка отсутствует. | 
| `Between`	| Закрашивается участок между ползунками. | 
| `Outside`	| Закрашиваются участки слева от LowerValue и справа от UpperValue. |

### Засечки и ограничители
| Свойство	| Тип	| По умолчанию	| Описание | 
| :--- | :--- | :--- | :--- |
| `TickStep`	| `GridLength`	| `Auto`	| Шаг засечек. `Auto` — автоматический подбор красивого шага; `N*` — ровно `N` промежутков на всём диапазоне; число — фиксированный шаг в единицах `Minimum`–`Maximum`. | 
| `TickBrush`	| `IBrush`	| `Black`	| Цвет засечек. | 
| `TickLength`	| `double`	| `5`	| Длина засечек в пикселях. |  
| `ShowEndStops`	| `bool`	| `true`	| Показывать ограничители на краях трека (вертикальные линии на `Minimum` и `Maximum`). | 

## Стилизация через классы
`RangeSlider` предоставляет несколько стилевых классов для удобной кастомизации.

| Класс	| Элемент	| Назначение | 
| :--- | :--- | :--- |
| `RangeSliderThumb`	| `Border`	| Оба ползунка (общие стили). | 
| `RangeSliderLowerThumb`	| `Border`	| Нижний ползунок. | 
| `RangeSliderUpperThumb`	| `Border`	| Верхний ползунок. | 
| `RangeSliderTextBox`	| `TextBox`	| Текстовые поля ввода. | 
| `RangeSliderTrack`	| `Canvas`	| Трек (область перетаскивания). | 

Пример стилизации:

```xml
<Style Selector="Border.RangeSliderThumb">
    <Setter Property="Width" Value="6" />
    <Setter Property="Background" Value="#007ACC" />
</Style>

<Style Selector="TextBox.RangeSliderTextBox">
    <Setter Property="FontSize" Value="12" />
    <Setter Property="Padding" Value="4" />
</Style>
```

## Поведение в приложении
- **Перетаскивание ползунков** — зажмите левую кнопку мыши на ползунке и перемещайте. Ползунки не могут пересекаться (соблюдается минимальный зазор `Step`).

- **Ввод с клавиатуры** — щёлкните в текстовое поле и введите число. Поддерживаются только цифры, знак минуса, точка и запятая. При вводе некорректного значения текст возвращается к последнему допустимому.

- **Интерактивность ползунков** — если `IsLowerThumbEnabled` или `IsUpperThumbEnabled` установлены в `false`, соответствующий ползунок и его текстовое поле блокируются (становятся неактивными).

- **Динамическое обновление** — все визуальные элементы (ползунки, заливка, засечки) автоматически обновляются при изменении значений через код или привязку.

---

# Toolbar и ToolbarGroup

`Toolbar` и `ToolbarGroup` — это специализированные контролы для построения панелей инструментов. Они обеспечивают единообразное оформление кнопок, разделителей и группировку элементов с подписями.

- **Toolbar** — горизонтальный контейнер для кнопок и разделителей с автоматическими отступами и стилями.
- **ToolbarGroup** — группирует несколько элементов (кнопки, переключатели) и отображает под ними текстовую подпись.

Оба контрола основаны на `ItemsControl` и могут содержать любые элементы, но для кнопок и разделителей уже предопределены удобные стили.

## Базовое использование

```xml
<controls:Toolbar>
    <Button Content="Открыть" />
    <Button Content="Сохранить" />
    <Separator />
    <controls:ToolbarGroup Header="Вид">
        <ToggleButton Content="🔍" />
        <ToggleButton Content="📋" />
    </controls:ToolbarGroup>
</controls:Toolbar>
```
По умолчанию элементы располагаются горизонтально с отступом 4 пикселя. Разделители (`Separator`) автоматически получают вертикальную черту.

Два горизонтальных тулбара один над другим:

```xml
<StackPanel>
    <controls:Toolbar Orientation="Horizontal">
        <Button Content="Открыть" />
        <Button Content="Сохранить" />
    </controls:Toolbar>
    <controls:Toolbar Orientation="Horizontal">
        <Button Content="Вырезать" />
        <Button Content="Копировать" />
        <Button Content="Вставить" />
    </controls:Toolbar>
</StackPanel>
```

---

# Toolbar

## Свойства Toolbar

| Свойство	| Тип	| По умолчанию	| Описание | 
| :--- | :--- | :--- | :--- |
| `ItemsPanel`	| `ItemsPanelTemplate`	| `tackPanel` (горизонтальный)	| Панель для размещения элементов. Можно заменить на WrapPanel для переноса. | 
| `Background`	| `IBrush`	| `#F5F5F5`	| Фон панели. | 
| `BorderBrush`	| `IBrush`	| `#F5F5F5`	| Цвет нижней границы (используется для отделения от содержимого). | 
| `MinHeight`	| `double`	| `32`	| Минимальная высота тулбара. | 
| `Wrap` | `bool` | `false` | Переносить обьекты на новую строку при нехватке места | 
| `Orientation` | `Orientation` | `Orientation.Horizontal` | Ориентация `Toolbar` в пространстве (горизонтально или вертикально) | 

## Стилизация Toolbar
По умолчанию кнопки и разделители внутри `Toolbar` уже стилизованы:

Кнопки: прозрачный фон, без границ, при наведении фон становится `#E0E0E0`.

Разделители: ширина `1px`, высота `20px`, цвет `#CCCCCC`, отступы `6px` слева и справа.

Для переопределения используйте селекторы:

```xml
<Style Selector="controls|Toolbar Button">
    <Setter Property="Padding" Value="10,4" />
</Style>

<Style Selector="controls|Toolbar Separator">
    <Setter Property="Background" Value="Gray" />
</Style>
```

Доступен стилевой класс `ToolbarRoot` на корневом `Border` темы, позволяющий изменить общий вид:

```xml
<Style Selector="Border.ToolbarRoot">
    <Setter Property="Background" Value="#2D2D30" />
</Style>
```

---

# ToolbarGroup
`ToolbarGroup` используется для логического объединения нескольких кнопок или переключателей с общей подписью, которая отображается под элементами. Это удобно для создания компактных панелей форматирования, выбора инструментов и т.п.

## Свойства ToolbarGroup
| Свойство	| Тип	| По умолчанию	| Описание | 
| :--- | :--- | :--- | :--- |
| `Header`	| `object?`	| `null`	| Содержимое подписи (обычно строка). Отображается под элементами группы. | 
| `ItemsPanel`	| `ItemsPanelTemplate`	| `StackPanel` (горизонтальный)	| Панель для размещения дочерних элементов. | 
| `Background`, `BorderBrush`, `BorderThickness`	| стандартные	| –	| Свойства корневого `Border`. | 

## Стилизация ToolbarGroup
Внутренние элементы (кнопки, разделители) стилизуются аналогично `Toolbar`. Дополнительно доступны классы:

| Класс	| Элемент	| Назначение| 
| :--- | :--- | :--- |
| `ToolbarGroupRoot`	| `Border`	| Корневой контейнер группы. | 
| `ToolbarGroupHeader`	| `ContentPresenter`	| Подпись группы. | 

Пример изменения подписи:

```xml
<Style Selector="ContentPresenter.ToolbarGroupHeader">
    <Setter Property="FontSize" Value="10" />
    <Setter Property="Foreground" Value="#555555" />
</Style>
```

Хотя `ToolbarGroup` рассчитан на кнопки, он может содержать любые элементы, но для них, вероятно, потребуется дополнительная стилизация.

## Рекомендации по использованию
Разделяйте логические блоки с помощью `Separator`.

Для переключаемых состояний используйте `ToggleButton` внутри `ToolbarGroup`.

Не используйте длинные компоненты для вертикального `Toolbar`, такие как текстовые поля или `ToolbarGroup` (он не имеет вертикального расположения), используйте кнопки в виде иконок и `CheckBox` без подписей.

Стилизуйте через селекторы глобально в `App.axaml`, чтобы сохранить единый вид всех тулбаров в приложении.
