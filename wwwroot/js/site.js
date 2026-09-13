// Read.me - Modern Reading Journal (ASP.NET Core MVC + jQuery Ajax)
$(document).ready(function () {

    // --- Toast Notification Helper ---
    function showToast(message, isSuccess = true) {
        var toastEl = $('#appToast');
        var toastBody = $('#appToastBody');
        toastBody.text(message);

        toastEl.removeClass('bg-danger bg-primary bg-success');
        if (isSuccess) {
            toastEl.addClass('bg-success');
        } else {
            toastEl.addClass('bg-danger');
        }

        var toast = new bootstrap.Toast(toastEl[0]);
        toast.show();
    }

    // --- 1. Global Book Search (jQuery Ajax) ---
    $('#btnExecuteSearch').on('click', function () {
        performSearch();
    });

    $('#inputBookSearch').on('keypress', function (e) {
        if (e.which === 13) {
            performSearch();
        }
    });

    function performSearch() {
        var query = $('#inputBookSearch').val().trim();
        if (!query) {
            alert('검색어를 입력해 주세요.');
            return;
        }

        $('#searchLoading').removeClass('d-none');
        $('#searchResultsList').empty();

        $.ajax({
            url: '/Search/Query',
            type: 'GET',
            data: { q: query },
            dataType: 'json',
            success: function (response) {
                $('#searchLoading').addClass('d-none');
                if (!response.success || !response.data || response.data.length === 0) {
                    $('#searchResultsList').html(`
                        <div class="col-12 text-center text-muted py-4">
                            <i class="bi bi-search fs-2"></i>
                            <p class="mt-2">검색 결과가 없습니다. 다른 검색어를 입력해 보세요.</p>
                        </div>
                    `);
                    return;
                }

                var html = '';
                $.each(response.data, function (index, book) {
                    var coverImg = book.coverImageUrl ? 
                        `<img src="${book.coverImageUrl}" alt="${book.title}" class="img-fluid rounded shadow-sm" style="max-height: 90px; object-fit: cover;" />` :
                        `<div class="bg-secondary text-white rounded d-flex align-items-center justify-content-center" style="height: 90px;"><i class="bi bi-book fs-3"></i></div>`;

                    html += `
                        <div class="col-12">
                            <div class="card border-0 bg-light p-3">
                                <div class="d-flex gap-3 align-items-center">
                                    <div class="flex-shrink-0 text-center" style="width: 70px;">
                                        ${coverImg}
                                    </div>
                                    <div class="flex-grow-1 min-w-0">
                                        <h6 class="fw-bold mb-1 text-truncate">${book.title}</h6>
                                        <p class="text-muted small mb-1">${book.author || '저자 미상'} | ${book.publisher || '출판사 미상'}</p>
                                        <p class="text-secondary small mb-0 text-truncate">${book.description || ''}</p>
                                    </div>
                                    <div class="flex-shrink-0">
                                        <button type="button" class="btn btn-primary btn-sm btn-add-library" 
                                            data-isbn="${book.isbn}" 
                                            data-title="${book.title}" 
                                            data-author="${book.author}" 
                                            data-publisher="${book.publisher}" 
                                            data-cover="${book.coverImageUrl}" 
                                            data-pages="${book.totalPages}" 
                                            data-desc="${book.description}">
                                            <i class="bi bi-bookmark-plus"></i> 서재에 담기
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    `;
                });

                $('#searchResultsList').html(html);
            },
            error: function () {
                $('#searchLoading').addClass('d-none');
                showToast('도서 검색 중 오류가 발생했습니다.', false);
            }
        });
    }

    // Add to library via Ajax
    $(document).on('click', '.btn-add-library', function () {
        var btn = $(this);
        var bookData = {
            isbn: btn.data('isbn'),
            title: btn.data('title'),
            author: btn.data('author'),
            publisher: btn.data('publisher'),
            coverImageUrl: btn.data('cover'),
            totalPages: parseInt(btn.data('pages')) || 300,
            description: btn.data('desc'),
            status: 1 // Reading
        };

        btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm"></span> 추가 중...');

        $.ajax({
            url: '/Search/AddToLibrary',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(bookData),
            success: function (res) {
                if (res.success) {
                    btn.removeClass('btn-primary').addClass('btn-success').html('<i class="bi bi-check"></i> 담기 완료');
                    showToast(res.message, true);
                    setTimeout(function () {
                        location.reload();
                    }, 1200);
                } else {
                    btn.prop('disabled', false).html('<i class="bi bi-bookmark-plus"></i> 서재에 담기');
                    showToast(res.message, false);
                }
            },
            error: function () {
                btn.prop('disabled', false).html('<i class="bi bi-bookmark-plus"></i> 서재에 담기');
                showToast('서재에 추가하지 못했습니다.', false);
            }
        });
    });

    // --- 2. Dashboard Quick Page Update (Ajax) ---
    $(document).on('click', '.btn-quick-save', function () {
        var btn = $(this);
        var id = btn.data('id');
        var inputPage = $(`#input-quick-page-${id}`);
        var page = parseInt(inputPage.val()) || 0;

        btn.prop('disabled', true).text('저장 중...');

        $.ajax({
            url: '/Books/UpdateProgress',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ userBookId: id, currentPage: page }),
            success: function (res) {
                btn.prop('disabled', false).text('저장');
                if (res.success) {
                    $(`#curr-page-text-${id}`).text(res.data.currentPage);
                    $(`#pct-text-${id}`).text(res.data.progressPercentage + '%');
                    $(`#progress-bar-${id}`).css('width', res.data.progressPercentage + '%');
                    showToast(res.message, true);
                } else {
                    showToast(res.message, false);
                }
            },
            error: function () {
                btn.prop('disabled', false).text('저장');
                showToast('진행률 저장 중 오류가 발생했습니다.', false);
            }
        });
    });

    // --- 3. Book Details Progress Slider (Ajax) ---
    var rangeProgress = $('#rangeProgress');
    var inputCurrentPage = $('#inputCurrentPage');
    var lblProgressPercentage = $('#lblProgressPercentage');
    var detailProgressBar = $('#detailProgressBar');

    if (rangeProgress.length) {
        var totalPages = parseInt(rangeProgress.attr('max')) || 300;

        rangeProgress.on('input', function () {
            var val = $(this).val();
            inputCurrentPage.val(val);
            var pct = Math.round(val / totalPages * 100);
            lblProgressPercentage.text(pct);
            detailProgressBar.css('width', pct + '%');
        });

        inputCurrentPage.on('input', function () {
            var val = Math.min(totalPages, Math.max(0, parseInt($(this).val()) || 0));
            rangeProgress.val(val);
            var pct = Math.round(val / totalPages * 100);
            lblProgressPercentage.text(pct);
            detailProgressBar.css('width', pct + '%');
        });

        $('#btnUpdateProgressAjax').on('click', function () {
            var btn = $(this);
            var id = btn.data('id');
            var page = parseInt(inputCurrentPage.val()) || 0;

            btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm"></span> 저장 중...');

            $.ajax({
                url: '/Books/UpdateProgress',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ userBookId: id, currentPage: page }),
                success: function (res) {
                    btn.prop('disabled', false).html('<i class="bi bi-check-lg"></i> 저장');
                    if (res.success) {
                        showToast(res.message, true);
                        if (res.data.status === 'Completed') {
                            $('#badge-detail-status').removeClass('bg-primary bg-warning').addClass('bg-success').text('Completed');
                        }
                    } else {
                        showToast(res.message, false);
                    }
                },
                error: function () {
                    btn.prop('disabled', false).html('<i class="bi bi-check-lg"></i> 저장');
                    showToast('진행률 업데이트에 실패했습니다.', false);
                }
            });
        });
    }

    // --- 4. Live Markdown Note Editor & Ajax Save ---
    var inputNoteThought = $('#inputNoteThought');
    var noteLivePreview = $('#noteLivePreview');

    if (inputNoteThought.length) {
        inputNoteThought.on('input', function () {
            var text = $(this).val();
            if (!text.trim()) {
                noteLivePreview.html('<em class="text-muted">입력한 내용이 실시간으로 여기에 렌더링됩니다.</em>');
                return;
            }

            // Simple fast client-side markdown render for live preview
            var html = text
                .replace(/^### (.*$)/gim, '<h6>$1</h6>')
                .replace(/^## (.*$)/gim, '<h5>$1</h5>')
                .replace(/^# (.*$)/gim, '<h4>$1</h4>')
                .replace(/^\> (.*$)/gim, '<blockquote class="blockquote small ps-2 border-start border-2">$1</blockquote>')
                .replace(/\*\*(.*?)\*\*/gim, '<strong>$1</strong>')
                .replace(/\*(.*?)\*/gim, '<em>$1</em>')
                .replace(/\n/gim, '<br />');

            noteLivePreview.html(html);
        });

        $('#btnSaveNoteAjax').on('click', function () {
            var userBookId = parseInt($('#noteUserBookId').val());
            var page = parseInt($('#inputNotePage').val()) || 1;
            var quote = $('#inputNoteQuote').val();
            var thought = $('#inputNoteThought').val();

            if (!thought.trim()) {
                alert('생각 & 감상 내용을 입력해 주세요.');
                return;
            }

            var btn = $(this);
            btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm"></span> 저장 중...');

            $.ajax({
                url: '/Notes/Create',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    userBookId: userBookId,
                    pageNumber: page,
                    quote: quote,
                    thought: thought
                }),
                success: function (res) {
                    btn.prop('disabled', false).html('<i class="bi bi-save"></i> 메모 저장 (Ajax)');
                    if (res.success) {
                        showToast(res.message, true);
                        $('#inputNoteQuote').val('');
                        $('#inputNoteThought').val('');
                        noteLivePreview.html('<em class="text-muted">입력한 내용이 실시간으로 여기에 렌더링됩니다.</em>');
                        $('#emptyNotesPlaceholder').remove();

                        var quoteHtml = res.data.quote ? 
                            `<blockquote class="blockquote fs-6 text-muted ps-3 border-start border-3 my-2 fst-italic">"${res.data.quote}"</blockquote>` : '';

                        var newNoteHtml = `
                            <div class="card border-0 shadow-sm p-3 border-start border-4 border-info" id="note-item-${res.data.id}">
                                <div class="d-flex justify-content-between align-items-center mb-2">
                                    <span class="badge bg-primary">p.${res.data.pageNumber}</span>
                                    <div class="d-flex align-items-center gap-2">
                                        <span class="text-muted small">${res.data.createdAt}</span>
                                        <button type="button" class="btn btn-outline-danger btn-sm py-0 px-1 btn-delete-note" data-id="${res.data.id}" title="메모 삭제">
                                            <i class="bi bi-x"></i>
                                        </button>
                                    </div>
                                </div>
                                ${quoteHtml}
                                <div class="markdown-body small text-dark mt-2">
                                    ${res.data.thoughtHtml}
                                </div>
                            </div>
                        `;

                        $('#notesContainer').prepend(newNoteHtml);
                        var countEl = $('#notesCount');
                        countEl.text(parseInt(countEl.text()) + 1);
                    } else {
                        showToast(res.message, false);
                    }
                },
                error: function () {
                    btn.prop('disabled', false).html('<i class="bi bi-save"></i> 메모 저장 (Ajax)');
                    showToast('메모 저장 중 오류가 발생했습니다.', false);
                }
            });
        });
    }

    // Delete Note via Ajax
    $(document).on('click', '.btn-delete-note', function () {
        if (!confirm('이 독서 메모를 삭제하시겠습니까?')) return;

        var btn = $(this);
        var id = btn.data('id');

        $.ajax({
            url: `/Notes/Delete?id=${id}`,
            type: 'POST',
            success: function (res) {
                if (res.success) {
                    $(`#note-item-${id}`).fadeOut(300, function () {
                        $(this).remove();
                        var countEl = $('#notesCount');
                        var nextCount = Math.max(0, parseInt(countEl.text()) - 1);
                        countEl.text(nextCount);
                    });
                    showToast(res.message, true);
                } else {
                    showToast(res.message, false);
                }
            },
            error: function () {
                showToast('메모 삭제에 실패했습니다.', false);
            }
        });
    });

    // --- 5. Export to README.md (Ajax) ---
    function triggerExportReadme() {
        $('#txtReadmeContent').val('README.md 마크다운을 생성하고 있습니다...');
        var modal = new bootstrap.Modal($('#exportReadmeModal')[0]);
        modal.show();

        $.ajax({
            url: '/Home/ExportReadme',
            type: 'GET',
            success: function (res) {
                if (res.success) {
                    $('#txtReadmeContent').val(res.data);
                } else {
                    $('#txtReadmeContent').val('마크다운 생성에 실패했습니다.');
                }
            },
            error: function () {
                $('#txtReadmeContent').val('서버와 통신 중 오류가 발생했습니다.');
            }
        });
    }

    $('#btnExportReadmeHeader, #btnExportReadmeHero').on('click', function () {
        triggerExportReadme();
    });

    $('#btnCopyReadmeMarkdown').on('click', function () {
        var content = $('#txtReadmeContent').val();
        if (!content) return;

        navigator.clipboard.writeText(content).then(function () {
            showToast('README 마크다운 코드가 클립보드에 복사되었습니다!', true);
        }).catch(function () {
            showToast('클립보드 복사에 실패했습니다.', false);
        });
    });

});
