<!DOCTYPE html>
<html>
<head>
    <title>Анализатор структур Rust</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; max-width: 800px; margin: 0 auto; padding: 20px; }
        code { background: #f4f4f4; padding: 2px 5px; border-radius: 3px; }
        pre { background: #f8f8f8; padding: 10px; border-radius: 5px; overflow-x: auto; }
        img { max-width: 100%; border: 1px solid #ddd; }
    </style>
</head>
<body>

<h1>Анализатор структур на языке Rust</h1>

<h2>Вариант 8: Объявление и определение структуры</h2>

<pre><code class="language-rust">struct Student {
    name: String,
    roll: u64,
    dept: String,
};</code></pre>

<h2>Примеры допустимых структур</h2>

<p><strong>1. Структура с полями базовых типов:</strong></p>
<pre><code class="language-rust">struct Student {
    name: String,
    roll: u64,
    dept: String,
};</code></pre>

<p><strong>2. Структура с числовыми полями:</strong></p>
<pre><code class="language-rust">struct Point {
    x: i32,
    y: i32,
};</code></pre>

<h2>Разработанная грамматика</h2>
<pre>
&lt;Def&gt;       → "struct " &lt;STRUCT&gt;
&lt;STRUCT&gt;    → ' ' &lt;Name&gt; 
&lt;Name&gt;      → &lt;Letter&gt; &lt;NameRem&gt;
&lt;NameRem&gt;   → &lt;Letter&gt; &lt;NameRem&gt;
            | &lt;Digit&gt; &lt;NameRem&gt;
            | '{' &lt;x&gt;
&lt;x&gt;         → &lt;Letter&gt; &lt;XRem&gt;
&lt;XRem&gt;      → &lt;Letter&gt; &lt;XRem&gt;
            | &lt;Digit&gt; &lt;XRem&gt;
            | ':' &lt;y&gt; 
&lt;y&gt;         → &lt;Type&gt; &lt;Field&gt; 
&lt;Field&gt;     → ',' &lt;x&gt;
            | '}' &lt;End&gt; 
&lt;End&gt;       → ";" 
&lt;Type&gt;      → "String" | "u64" | "i32" | "f64" | "bool"
&lt;Letter&gt;    → [a-zA-Z]
&lt;Digit&gt;     → [0-9]
</pre>

<h2>Классификация грамматики</h2>

<p>Согласно классификации Хомского, данная грамматика:</p>
<ul>
    <li>Тип 3 (регулярная/автоматная)</li>
    <li>Все продукции праворекурсивные (A → aB | a | ε)</li>
</ul>

<h2>Граф конечного автомата</h2>

<img src="Рисунок2.png" alt="Граф конечного автомата">

<h2>Тестовые примеры</h2>

<h3>Корректный ввод</h3>
<img src="Рисунок8.png" alt="Пример корректной структуры">

<h3>Ошибочный ввод</h3>
<img src="Рисунок6.png" alt="Пример вывода ошибок">

<h3>Ошибочный ввод</h3>
<img src="Рисунок7.png" alt="Пример вывода ошибок">


<h2>Использование</h2>
<ol>
    <li>Введите код структуры в редактор</li>
    <li>Нажмите "Анализировать"</li>
    <li>Просмотрите результаты:
        <ul>
            <li>Все ошибки будут выделены</li>
            <li>Подробный отчет о синтаксических ошибках</li>
        </ul>
    </li>
</ol>
</body>
</html>